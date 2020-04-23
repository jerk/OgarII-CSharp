using System;
using System.Collections.Generic;
using System.Text;
using Ogar_CSharp.Cells;
using Ogar_CSharp.Other;
using Ogar_CSharp.Sockets;
using Ogar_CSharp.Worlds;
using System.Linq;
using System.Drawing;

namespace Ogar_CSharp.Protocols
{
    public class ModernProtocol : Protocol
    {
        readonly byte[] pingReturn = new byte[1] { 2 };
        public uint protocolVersion;
        public bool gotProtocol;
        public LeaderBoardEntry leaderboardSelfData;
        public IEnumerable<LeaderBoardEntry> leaderboardData;
        public LeaderboardType? leaderboardType;
        public Queue<(ChatChannel.ChatSource source, string message)> chatPending = new Queue<(ChatChannel.ChatSource source, string message)>();
        public RectangleF? worldBorderPending;
        public ViewArea? spectateAreaPending;
        public bool serverInfoPending, worldStatsPending, clearCellsPending, leaderboardPending;
        private ModernProtocol(Connection connection) : base(connection)
        {

        }
        public override string Type => "modern";

        public override string SubType
        {
            get
            {
                var str = "//";
                if(gotProtocol)
                {
                    str = "00" + protocolVersion;
                    str = str[0..^2];
                }
                return str;
            }
        }

        internal static Protocol Decider(DataReader reader, Connection connection)
        {
            if (reader.length < 5)
                return null;
            var val = reader.Read<byte>();
            if (val != 1)
                return null;
            uint ver = reader.Read<uint>();
            if (ver != 3)
            {
                connection.CloseSocket(1003, "Unsupported protocol version");
                return null;
            }
            return new ModernProtocol(connection) { gotProtocol = true, protocolVersion = ver };
        }
        public override void OnSpectatePosition(ViewArea area)
        {
            spectateAreaPending = area;
        }
        public override void OnSocketMessage(DataReader reader)
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
                    connection.mouseX = reader.Read<int>();
                    connection.mouseY = reader.Read<int>();
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
        public override void OnNewWorldBounds(RectangleF range, bool includeServerInfo)
        {
            worldBorderPending = range;
            serverInfoPending = includeServerInfo;
        }
        public override void OnVisibleCellUpdate(IList<Cell> add, IList<Cell> upd, IList<Cell> eat, IList<Cell> del)
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
            if (chatPending.Count > 0)
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
            writer.Write(globalFlags);
            if (spectateAreaPending != null)
            {
                writer.Write(spectateAreaPending.Value.x);
                writer.Write(spectateAreaPending.Value.y);
                writer.Write(spectateAreaPending.Value.s);
                spectateAreaPending = null;
            }
            if (worldBorderPending != null)
            {
                var item = worldBorderPending.Value;
                writer.Write(item.X - item.Width);
                writer.Write(item.X + item.Width);
                writer.Write(item.Y - item.Height);
                writer.Write(item.Y + item.Height);
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
                writer.Write((float)item.loadTime / this.Handle.tickDelay);
                writer.Write((uint)item.uptime);
                writer.Write((ushort)item.limit);
                writer.Write((ushort)item.external);
                writer.Write((ushort)item._internal);
                writer.Write((ushort)item.playing);
                writer.Write((ushort)item.spectating);
                worldStatsPending = false;
            }
            if ((l = chatPending.Count) > 0)
            {
                writer.Write((ushort)l);
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
                        IList<FFALeaderboardEntry> entries = (IList<FFALeaderboardEntry>)leaderboardData;
                        for (int j = 0; j < entries.Count; j++)
                        {
                            var entry = entries[j];
                            flags = 0;
                            if (entry.highlighted)
                                flags |= 1;
                            if (entry == leaderboardSelfData)
                            {
                                flags |= 2;
                                hitSelfData = true;
                            }
                            writer.Write((ushort)entry.position);
                            writer.WriteByte(flags);
                            writer.WriteUTF8String(entry.name);
                        }
                        FFALeaderboardEntry item;
                        if (!hitSelfData && (item = (FFALeaderboardEntry)leaderboardSelfData) != null)
                        {
                            writer.Write((ushort)item.position);
                            flags = (byte)(item.highlighted ? 1 : 0);
                            writer.WriteByte(flags);
                            writer.WriteUTF8String(item.name);
                        }
                        writer.Write<ushort>(0);
                        break;
                    case LeaderboardType.Pie:
                        writer.WriteByte(2);
                        writer.Write((ushort)l);
                        IList<PieLeaderboardEntry> entries1 = (IList<PieLeaderboardEntry >)leaderboardData;
                        for (int j = 0; j < entries1.Count; j++)
                            writer.Write(entries1[j].weight);
                        break;
                    case LeaderboardType.Text:
                        writer.WriteByte(3);
                        writer.Write((ushort)l);
                        IList<TextLeaderBoardEntry> entries2 = (IList<TextLeaderBoardEntry>)leaderboardData;
                        for (int j = 0; j < entries2.Count; j++)
                            writer.WriteUTF8String(entries2[j].text);
                        break;
                }
                leaderboardPending = false;
                leaderboardType = null;
                leaderboardData = null;
                leaderboardSelfData = null;
            }
            if (add.Count > 0)
            {
                for (int s = 0; s < add.Count; s++)
                {
                    var item = add[s];
                    writer.Write(item.id);
                    writer.WriteByte(item.Type);
                    writer.Write(item.X);
                    writer.Write(item.X);
                    writer.Write((ushort)item.Size);
                    writer.WriteColor((uint)item.Color);
                    flags = 0;
                    if (item.Type == 0 && item.owner == connection.Player)
                        flags |= 0;
                    if (item.Name != null)
                        writer.WriteUTF8String(item.Name);
                    if (item.Skin != null)
                        writer.WriteUTF8String(item.Skin);
                }
                writer.Write<uint>(0);
            }
            if (upd.Count > 0)
            {
                for (int s = 0; s < upd.Count; s++)
                {
                    var item = upd[s];
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
                    writer.Write(item.id);
                    writer.WriteByte(flags);
                    if (item.posChanged)
                    {
                        writer.Write(item.X);
                        writer.Write(item.X);
                    }
                    if (item.skinChanged)
                        writer.Write((ushort)item.Size);
                    if (item.colorChanged)
                        writer.WriteColor((uint)item.Color);
                    if (item.nameChanged)
                        writer.WriteUTF8String(item.Name);
                    if (item.sizeChanged)
                        writer.WriteUTF8String(item.Skin);
                }
                writer.Write<uint>(0);
            }
            if (eat.Count > 0)
            {
                for (int s = 0; s < eat.Count; s++)
                {
                    var item = eat[s];
                    writer.Write(item.id);
                    writer.Write(item.eatenBy.id);
                }
                writer.Write<uint>(0);
            }
            if (del.Count > 0)
            {
                for (int s = 0; s < del.Count; s++)
                    writer.Write(del[s].id);
                writer.Write<uint>(0);
            }
            Send(writer.ToArray());
        }
        public override void OnWorldReset()
        {
            clearCellsPending = true;
            worldBorderPending = null;
            worldStatsPending = false;
            var empty = new List<Cell>();
            OnVisibleCellUpdate(empty, empty, empty, empty);
        }
        public override void OnLeaderboardUpdate<T>(LeaderboardType type, IList<T> data, LeaderBoardEntry selfData)
        {
            leaderboardPending = true;
            leaderboardType = type;
            leaderboardData = data;
            leaderboardSelfData = selfData;
        }
    }
}
