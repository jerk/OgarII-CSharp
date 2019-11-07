using System;
using System.Collections.Generic;
using System.Text;
using Ogar_CSharp.Cells;
using Ogar_CSharp.Other;
using Ogar_CSharp.Sockets;
using Ogar_CSharp.Worlds;
using System.Linq;

namespace Ogar_CSharp.Protocols
{
    public class ModernProtocol : Protocol
    {
        readonly byte[] pingReturn = new byte[1] { 2 };
        public uint? protocol;
        public bool gotProtocol;
        public LeaderBoardEntry leaderboardSelfData;
        public IEnumerable<LeaderBoardEntry> leaderboardData;
        public LeaderboardType? leaderboardType;
        public Queue<(ChatChannel.ChatSource source, string message)> chatPending = new Queue<(ChatChannel.ChatSource source, string message)>();
        public Rect? worldBorderPending;
        public ViewArea? spectateAreaPending;
        public bool serverInfoPending, worldStatsPending, clearCellsPending, leaderboardPending;
        public ModernProtocol(Connection connection) : base(connection)
        {

        }
        public override string Type => "modern";

        public override string SubType
        {
            get
            {
                var str = "//";
                if(protocol != null)
                {
                    str = "00" + protocol.Value;
                    str = str[0..^2];
                }
                return str;
            }
        }
        public override bool Distinguishes(Reader reader)
        {
            if (reader.length < 5)
                return false;
            var val = reader.ReadByte();
            if (val != 1)
                return false;
            gotProtocol = true;
            Console.WriteLine("protocol "  + (protocol = reader.ReadUInt()));
            if (protocol != 3)
            {
                Fail(1003, "Unsupported protocol version");
                return false;
            }
            connection.CreatePlayer();
            return true;
        }
        public override void OnSpectatePosition(ViewArea area)
        {
            spectateAreaPending = area;
        }
        public override void OnSocketMessage(Reader reader)
        {
            byte messageId = reader.ReadByte();
            switch (messageId)
            {
                case 2:
                    Send(pingReturn);
                    worldStatsPending = true;
                    break;
                case 3:
                    if (reader.length < 12)
                    {
                        Fail(1003, "Unexpected message format");
                        return;
                    }
                    int i, l, count;
                    connection.mouseX = reader.ReadInt();
                    connection.mouseY = reader.ReadInt();
                    connection.splitAttempts += reader.ReadByte();
                    count = reader.ReadByte();
                    for (i = 0, l = connection.minions.Count; count > 0 && i < l; i++)
                        connection.minions[i].splitAttempts += count;
                    byte globalFlags = reader.ReadByte();
                    if((globalFlags & 1) != 0)
                    {
                        if(reader.length < 13)
                        {
                            Fail(1003, "Unexpected message format");
                            return;
                        }
                        connection.spawningName = reader.ReadUTF8String();
                    }
                    if ((globalFlags & 2) != 0) 
                        this.connection.requestingSpectate = true;
                    if ((globalFlags & 4) != 0) 
                        this.connection.isPressingQ = true;
                    if ((globalFlags & 8) != 0) 
                        this.connection.isPressingQ = this.connection.hasProcessedQ = false;
                    if ((globalFlags & 16) != 0) 
                        this.connection.ejectAttempts++;
                    if ((globalFlags & 32) != 0)
                        for (i = 0, l = connection.minions.Count; i < l; i++)
                            connection.minions[i].ejectAttempts++;
                    if ((globalFlags & 64) != 0) 
                        this.connection.minionsFrozen = !this.connection.minionsFrozen;
                    if((globalFlags & 128) != 0)
                    {
                        if(reader.length < 13 + (globalFlags & 1)  + count)
                        {
                            Fail(1003, "Unexpected message format");
                            return;
                        }
                        for (i = 0; i < count; i++)
                            connection.OnChatMessage(reader.ReadUTF8String());
                    }
                    break;
                default:
                    {
                        Fail(1003, "Unknown message type");
                        return;
                    }
            }
        }
        public void OnChatMessage(ChatChannel.ChatSource source, string message)
            => chatPending.Enqueue((source, message));
        public override void OnNewOwnedCell(PlayerCell cell)
        {
            //Ignored
        }
        public override void OnNewWorldBounds(Rect range, bool includeServerInfo)
        {
            worldBorderPending = range;
            serverInfoPending = includeServerInfo;
        }
        public override void OnVisibleCellUpdate(IEnumerable<Cell> add, IEnumerable<Cell> upd, IEnumerable<Cell> eat, IEnumerable<Cell> del)
        {
            int i, l;
            bool hitSelfData = false;
            ushort globalFlags = 0;
            byte flags = 0;
            if (spectateAreaPending != null)
                globalFlags |= 1;
            if (worldBorderPending != null)
                globalFlags |= 2;
            if (serverInfoPending)
                globalFlags |= 2;
            if (connection.hasPlayer && connection.Player.hasWorld && worldStatsPending)
                globalFlags |= 8;
            if(chatPending.Count > 0)
                globalFlags |= 16;
            if (leaderboardPending)
                globalFlags |= 32;
            if (clearCellsPending)
                globalFlags |= 64;
            if (add.Count() > 0)
                globalFlags |= 128;
            if (upd.Count() > 0)
                globalFlags |= 258;
            if (eat.Count() > 0)
                globalFlags |= 512;
            if (del.Count() > 0)
                globalFlags |= 1024;
            if (globalFlags == 0)
                return;
            var writer = new Writer();
            writer.WriteByte(3);
            writer.WriteUShort(globalFlags);
            if(spectateAreaPending != null)
            {
                writer.WriteFloat(spectateAreaPending.Value.x);
                writer.WriteFloat(spectateAreaPending.Value.y);
                writer.WriteFloat(spectateAreaPending.Value.s);
                spectateAreaPending = null;
            }
            if(worldBorderPending != null)
            {
                var item = worldBorderPending.Value;
                writer.WriteFloat(item.x - item.w);
                writer.WriteFloat(item.x + item.w);
                writer.WriteFloat(item.y - item.h);
                writer.WriteFloat(item.y + item.h);
                worldBorderPending = null;
            }
            if (serverInfoPending)
            {
                writer.WriteByte(Handle.gamemode.Type);
                var item = Handle.Version.Split(".");
                writer.WriteByte(byte.Parse(item[0]));
                writer.WriteByte(byte.Parse(item[1]));
                writer.WriteByte(byte.Parse(item[2]));
                serverInfoPending = false;
            }
            if (worldStatsPending)
            {
                var item = connection.Player.world.stats;
                writer.WriteUTF8String(item.name);
                writer.WriteUTF8String(item.gamemode);
                writer.WriteFloat((float)item.loadTime / (float)this.Handle.tickDelay);
                writer.WriteUInt((uint)item.uptime);
                writer.WriteUShort((ushort)item.limit);
                writer.WriteUShort((ushort)item.external);
                writer.WriteUShort((ushort)item._internal);
                writer.WriteUShort((ushort)item.playing);
                writer.WriteUShort((ushort)item.spectating);
                worldStatsPending = false;
            }
            if((l = chatPending.Count) > 0)
            {
                writer.WriteUShort((ushort)l);
                for (i = 0; i < l; i++)
                {
                    var item = this.chatPending.Dequeue();
                    writer.WriteUTF8String(item.source.name);
                    writer.WriteColor((uint)item.source.color);
                    writer.WriteByte((byte)((item.source.isServer != null) ? 1 : 0));
                    writer.WriteUTF8String(item.message);
                }

            }
            if (leaderboardPending)
            {
                l = leaderboardData.Count();
                switch (leaderboardType.Value)
                {
                    case LeaderboardType.FFA:
                        writer.WriteByte(1);
                        IEnumerable<FFALeaderboardEntry> entries = leaderboardData.Cast<FFALeaderboardEntry>();
                        foreach (FFALeaderboardEntry entry in entries)
                        {
                            flags = 0;
                            if (entry.highlighted)
                                flags |= 1;
                            if (entry == leaderboardSelfData)
                            {
                                flags |= 2;
                                hitSelfData = true;
                            }
                            writer.WriteUShort((ushort)entry.position);
                            writer.WriteByte(flags);
                            writer.WriteUTF8String(entry.name);
                        }
                        FFALeaderboardEntry item;
                        if(!hitSelfData && (item = (FFALeaderboardEntry)leaderboardSelfData) != null)
                        {
                            writer.WriteUShort((ushort)item.position);
                            flags = (byte)(item.highlighted ? 1 : 0);
                            writer.WriteByte(flags);
                            writer.WriteUTF8String(item.name);
                        }
                        writer.WriteUShort(0);
                        break;
                    case LeaderboardType.Pie:
                        writer.WriteByte(2);
                        writer.WriteUShort((ushort)l);
                        IEnumerable<PieLeaderboardEntry> entries1 = leaderboardData.Cast<PieLeaderboardEntry>();
                        foreach(var entry in entries1)
                            writer.WriteFloat(entry.weight);
                        break;
                    case LeaderboardType.Text:
                        writer.WriteByte(3);
                        writer.WriteUShort((ushort)l);
                        IEnumerable<TextLeaderBoardEntry> entries2 = leaderboardData.Cast<TextLeaderBoardEntry>();
                        foreach (var entry in entries2)
                            writer.WriteUTF8String(entry.text);
                        break;
                }
                leaderboardPending = false;
                leaderboardType = null;
                leaderboardData = null;
                leaderboardSelfData = null;
            }
            if(add.Count() > 0)
            {
                foreach(var item in add)
                {
                    writer.WriteUInt((uint)item.id);
                    writer.WriteByte(item.Type);
                    writer.WriteFloat(item.X);
                    writer.WriteFloat(item.X);
                    writer.WriteUShort((ushort)item.Size);
                    writer.WriteColor((uint)item.Color);
                    flags = 0;
                    if (item.Type == 0 && item.owner == connection.Player)
                        flags |= 0;
                    if (item.Name != null)
                        writer.WriteUTF8String(item.Name);
                    if (item.Skin != null)
                        writer.WriteUTF8String(item.Skin);
                }
                writer.WriteUInt(0);
            }
            if(upd.Count() > 0)
            {
                foreach(var item in upd)
                {
                    flags = 0;
                    if (item.posChanged)
                        flags |= 1;
                    if (item.sizeChanged)
                        flags |= 2;
                    if (item.colorChanged)
                        flags |= 4;
                    if (item.nameChanged)
                        flags |= 8;
                    if (item.sizeChanged)
                        flags |= 16;
                    writer.WriteUInt((uint)item.id);
                    writer.WriteByte(flags);
                    if (item.posChanged)
                    {
                        writer.WriteFloat(item.X);
                        writer.WriteFloat(item.X);
                    }
                    if (item.skinChanged)
                        writer.WriteUShort((ushort)item.Size);
                    if (item.colorChanged)
                        writer.WriteColor((uint)item.Color);
                    if (item.nameChanged)
                        writer.WriteUTF8String(item.Name);
                    if (item.sizeChanged)
                        writer.WriteUTF8String(item.Skin);
                }
                writer.WriteUInt(0);
            }
            if(eat.Count() > 0)
            {
                foreach(var item in eat)
                {
                    writer.WriteUInt((uint)item.id);
                    writer.WriteUInt((uint)item.eatenBy.id);
                }
                writer.WriteUInt(0);
            }
            if(del.Count() > 0)
            {
                foreach (var item in del)
                    writer.WriteUInt((uint)item.id);
                writer.WriteUInt(0);
            }
            Send(writer.RawBuffer);
        }
        public override void OnWorldReset()
        {
            clearCellsPending = true;
            worldBorderPending = null;
            worldStatsPending = false;
            var empty = new List<Cell>();
            OnVisibleCellUpdate(empty, empty, empty, empty);
        }
        public override void OnLeaderboardUpdate(LeaderboardType type, IEnumerable<LeaderBoardEntry> data, LeaderBoardEntry selfData)
        {
            leaderboardPending = true;
            leaderboardType = type;
            leaderboardData = data;
            leaderboardSelfData = selfData;
        }
    }
}
