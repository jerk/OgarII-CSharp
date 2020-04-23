using System;
using System.Collections.Generic;
using System.Text;
using Ogar_CSharp.Cells;
using Ogar_CSharp.Other;
using Ogar_CSharp.Sockets;
using System.Linq;
using Ogar_CSharp.Worlds;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

namespace Ogar_CSharp.Protocols
{
    public struct Legacy
    {
        public string mode;
        public string name;
        public double update;
        public double uptime;
        public int playersTotal;
        public int playersAlive;
        public int playersSpect;
        public int playersLimit;
    }
    public class LegacyProtocol : Protocol
    {
        public bool gotProtocol = false;
        public uint protocolVersion;
        public bool gotKey = false;
        public uint? Key;
        public LeaderboardType? LastleaderboardType;
        public bool hasProcessedQ;
        private LegacyProtocol(Connection connection) : base(connection) { }
        public override string Type => "legacy";

        public override string SubType
        {
            get
            {
                var str = "//";
                if (gotProtocol)
                    str = ("00" + protocolVersion)[0..^2];
                return str;
            }
        }
        internal static Protocol Decider(DataReader reader, Connection connection)
        {
            if (reader.length < 5)
                return null;
            if (reader.Read<byte>() != 254)
                return null;
            return new LegacyProtocol(connection) { gotProtocol = true, protocolVersion = reader.Read<uint>() };
        }
        public override void OnLeaderboardUpdate<T>(LeaderboardType type, IList<T> data, LeaderBoardEntry selfData)
        {
            this.LastleaderboardType = type;
            var writer = new Writer();
            switch (type)
            {
                case LeaderboardType.FFA: 
                    FFALeaderboard(writer, (IList<FFALeaderboardEntry>)data, (FFALeaderboardEntry)selfData, protocolVersion); 
                    break;
                case LeaderboardType.Pie: 
                    PieLeaderboard(writer, (IList<PieLeaderboardEntry>)data, (PieLeaderboardEntry)selfData, protocolVersion); 
                    break;
                case LeaderboardType.Text: 
                    TextBoard(writer, (IList<TextLeaderBoardEntry>)data, protocolVersion);
                    break;
            }
            this.Send(writer.ToArray());
        }

        public override void OnNewOwnedCell(PlayerCell cell)
        {
            var writer = new Writer();
            writer.WriteByte(32);
            writer.Write(cell.id);
            Send(writer.ToArray());
        }
   

        public override void OnNewWorldBounds(RectangleF range, bool includeServerInfo)
        {
            var writer = new Writer();
            writer.WriteByte(64);
            writer.Write((double)(range.X - range.Width));
            writer.Write((double)(range.Y - range.Height));
            writer.Write((double)(range.X + range.Width));
            writer.Write((double)(range.Y + range.Height));
            if (includeServerInfo)
            {
                writer.Write<uint>(Handle.gamemode.Type);
                WriteZTString(writer, $"OgarII-CSharp {Handle.Version}", protocolVersion);
            }
            this.Send(writer.ToArray());
        }

        public override void OnSocketMessage(DataReader reader)
        {
            var messageId = reader.Read<byte>();
            if (!this.gotKey)
            {
                if (messageId != 255) return;
                if (reader.length < 5) { this.Fail(0, "Unexpected message format"); return; };
                this.gotKey = true;
                this.Key = reader.Read<uint>();
                this.connection.CreatePlayer();
                return;
            }
            switch (messageId)
            {
                case 0:
                    connection.spawningName = ReadZTString(reader, protocolVersion);
                    Console.WriteLine(connection.spawningName + "u");
                    break;
                case 1:
                    this.connection.requestingSpectate = true;
                    break;
                case 16:
                    switch (reader.length)
                    {
                        case 13:
                            this.connection.mouseX = reader.Read<int>();
                            this.connection.mouseY = reader.Read<int>();
                            break;
                        case 9:
                            this.connection.mouseX = reader.Read<short>();
                            this.connection.mouseY = reader.Read<short>();
                            break;
                        case 21:
                            this.connection.mouseX = (float)Math.Floor(reader.Read<double>());
                            this.connection.mouseY = (float)Math.Floor(reader.Read<double>());
                            break;
                        default: this.Fail(1003, "Unexpected message format");
                            return;
                    }
                    break;
                case 17:
                    if (this.connection.controllingMinions)
                        for (int i = 0, l = this.connection.minions.Count; i < l; i++)
                            this.connection.minions[i].splitAttempts++;
                    else if (connection.splitAttempts != byte.MaxValue)
                        this.connection.splitAttempts++;
                    break;
                case 18: this.connection.isPressingQ = true; break;
                case 19: this.connection.isPressingQ = this.hasProcessedQ = false; break;
                case 21:
                    if (this.connection.controllingMinions)
                        for (int i = 0, l = this.connection.minions.Count; i < l; i++)
                            this.connection.minions[i].ejectAttempts++;
                    else this.connection.ejectAttempts++;
                    break;
                case 22:
                    if (!this.gotKey || !Settings.minionEnableERTPControls) break;
                    for (int i = 0, l = this.connection.minions.Count; i < l; i++)
                        this.connection.minions[i].splitAttempts++;
                    break;
                case 23:
                    if (!this.gotKey || !Settings.minionEnableERTPControls) break;
                    for (int i = 0, l = this.connection.minions.Count; i < l; i++)
                        this.connection.minions[i].ejectAttempts++;
                    break;
                case 24:
                    if (!this.gotKey || !Settings.minionEnableERTPControls) break;
                    this.connection.minionsFrozen = !this.connection.minionsFrozen;
                    break;
                case 99:
                    if (reader.length < 2)
                    {
                        this.Fail(1003, "Bad message format");
                        return;
                    }
                    var flags = reader.Read<byte>();
                    var skipLen = 2 * ((flags & 2) + (flags & 4) + (flags & 8));
                    if (reader.length < 2 + skipLen) {
                        Fail(1003, "Unexpected message format");
                        return;
                    }
                    reader.Skip(skipLen);
                    var message = ReadZTString(reader, protocolVersion);
                    this.connection.OnChatMessage(message);
                    break;
                case 254:
                    if (this.connection.hasPlayer && this.connection.Player.hasWorld)
                        this.OnStatsRequest();
                    break;
                case 255:
                    Fail(1003, "Unexpected message");
                    return;
                default:
                    Fail(1003, "Unknown message type");
                    return;
            }
        }

        public override void OnSpectatePosition(ViewArea area)
        {
            var writer = new Writer();
            writer.WriteByte(17);
            writer.Write(area.x);
            writer.Write(area.y);
            writer.Write(area.s);
            Send(writer.ToArray());
        }
        public static void WriteCellData(Writer writer, Player source, uint protocol, Cell cell, bool includeType, bool includeSize,
            bool includePos, bool includeColor, bool includeName, bool includeSkin)
        {
            if (protocol == 4 || protocol == 5)
                WriteCellData4(writer, source, protocol, cell, includeType, includeSize, includePos, includeColor, includeName, includeSkin);
            else if (protocol <= 10)
                WriteCellData6(writer, source, protocol, cell, includeType, includeSize, includePos, includeColor, includeName, includeSkin);
            else if (protocol <= 22)
                WriteCellData11(writer, source, protocol, cell, includeType, includeSize, includePos, includeColor, includeName, includeSkin);
        }
        public override void OnVisibleCellUpdate(IList<Cell> add, IList<Cell> upd, IList<Cell> eat, IList<Cell> del)
        {
            var source = this.connection.Player;
            var writer = new Writer();
            writer.WriteByte(16);
            writer.Write((ushort)eat.Count);
            for (int i = 0; i < eat.Count; i++)
            {
                var item = eat[i];
                writer.Write(item.eatenBy.id);
                writer.Write(item.id);
            }
            for (int i = 0; i < add.Count; i++)
            {
                var item = add[i];
                WriteCellData(writer, source, protocolVersion, item,
                    true, true, true, true, true, true);
            }
            for (int i = 0; i < upd.Count; i++)
            {
                var item = upd[i];
                WriteCellData(writer, source, protocolVersion, item,
                    false, item.sizeChanged, item.posChanged, item.colorChanged, item.nameChanged, item.skinChanged);
            }
            writer.Write((uint)0);
            if (protocolVersion < 6)
                writer.Write((uint)del.Count);
            else
                writer.Write((ushort)del.Count);
            for (int i = 0; i < del.Count; i++)
                writer.Write(del[i].id);
            this.Send(writer.ToArray());
        } 
        public override void OnWorldReset()
        {
            Send(new byte[1] { 18 });
            if (LastleaderboardType != null)
            {
                this.OnLeaderboardUpdate(LastleaderboardType.Value, new List<LeaderBoardEntry>(), null);
                this.LastleaderboardType = null;
            }
        }
        private bool isCompilingStats;
        public void OnStatsRequest()
        {
            if (!isCompilingStats)
            {
                isCompilingStats = true;
                var writer = new Writer();
                writer.WriteByte(254);
                var stats = connection.Player.world.stats;
                var legacy = new Legacy
                {
                    name = stats.name,
                    mode = stats.gamemode,
                    update = stats.loadTime,
                    playersTotal = stats.external,
                    playersAlive = stats.playing,
                    playersSpect = stats.spectating,
                    playersLimit = stats.limit,
                    uptime = stats.uptime
                };
                Task.Run(() =>
                {
                    WriteZTString(writer, Newtonsoft.Json.JsonConvert.SerializeObject(legacy), protocolVersion);
                    Send(writer.ToArray());
                    isCompilingStats = false;
                });
            }
        }
        public static void PieLeaderboard(Writer writer, IList<PieLeaderboardEntry> data, PieLeaderboardEntry selfData, uint protocol)
        {
            if (protocol <= 20)
                PieLeaderboard4(writer, data, selfData, protocol);
            else if (protocol <= 22)
                PieLeaderboard21(writer, data, selfData, protocol);
        }
        public static void PieLeaderboard4(Writer writer, IList<PieLeaderboardEntry> data, PieLeaderboardEntry selfData, uint protocol)
        {
            writer.WriteByte(50);
            writer.Write((uint)data.Count);
            for (int i = 0, l = data.Count; i < l; i++)
                writer.Write(data[i].weight);
        }
        public static void PieLeaderboard21(Writer writer, IList<PieLeaderboardEntry> data, PieLeaderboardEntry selfData, uint protocol)
        {
            writer.WriteByte(51);
            writer.Write((uint)data.Count);
            for (int i = 0, l = data.Count; i < l; i++)
            {
                writer.Write(data[i].weight);
                writer.WriteColor((uint)data[i].color);
            }
        }
        public static void TextBoard(Writer writer, IList<TextLeaderBoardEntry> data, uint protocol)
        {
            if (protocol <= 13)
                TextBoard4(writer, data, protocol);
            else if (protocol <= 22)
                TextBoard14(writer, data, protocol);
        }
        public static void TextBoard4(Writer writer, IList<TextLeaderBoardEntry> data, uint protocol)
        {
            writer.WriteByte(48);
            writer.Write((uint)data.Count);
            for (int i = 0, l = data.Count; i < l; i++)
                WriteZTString(writer, data[i].text, protocol);
        }
        public static void TextBoard14(Writer writer, IList<TextLeaderBoardEntry> data, uint protocol)
        {
            writer.WriteByte(53);
            for (int i = 0, l = data.Count; i < l; i++)
            {
                writer.WriteByte(2);
                WriteZTString(writer, data[i].text, protocol);
            }
        }
        public static void FFALeaderboard(Writer writer, IList<FFALeaderboardEntry> data, FFALeaderboardEntry selfdata, uint protocol)
        {
            if (protocol <= 10)
                FFALeaderBoard4(writer, data, selfdata, protocol);
            else if (protocol <= 22)
                FFALeaderBoard11(writer, data, selfdata, protocol);
        }
        public static void FFALeaderBoard4(Writer writer, IList<FFALeaderboardEntry> data, FFALeaderboardEntry selfdata, uint protocol)
        {
            writer.WriteByte(49);
            writer.Write((uint)data.Count);
            for (int i = 0, l = data.Count; i < l; i++)
            {
                var item = data[i];
                if (protocol == 6)
                    writer.Write((uint)(item.highlighted ? 1 : 0));
                else 
                    writer.Write((uint)item.cellId);
                WriteZTString(writer, item.name, protocol);
            }
        }
        public static void FFALeaderBoard11(Writer writer, IList<FFALeaderboardEntry> data, FFALeaderboardEntry selfdata, uint protocol)
        {
            writer.WriteByte((byte)(protocol >= 14 ? 53 : 51));
            for (int i = 0, l = data.Count; i < l; i++)
            {
                var item = data[i];
                if (item == selfdata)
                    writer.WriteByte(8);
                else
                {
                    writer.WriteByte(2);
                    writer.WriteUTF8String(item.name);
                }
            }
        }
        public static void WriteCellData4(Writer writer, Player source, uint protocol, Cell cell, bool includeType, bool includeSize,
            bool includePos, bool includeColor, bool includeName, bool includeSkin)
        {
            writer.Write(cell.id);
            if(protocol == 4)
            {
                writer.Write((short)cell.X);
                writer.Write((short)cell.Y);
            }
            else
            {
                writer.Write((int)cell.X);
                writer.Write((int)cell.Y);
            }

            writer.Write((ushort)cell.Size);
            writer.WriteColor((uint)cell.Color);

            byte flags = 0;
            if (cell.IsSpiked) flags |= 0x01;
            if (includeSkin) flags |= 0x04;
            if (cell.IsAgitated) flags |= 0x10;
            if (cell.Type == 3) flags |= 0x20;
            writer.WriteByte(flags);

            if (includeSkin) writer.WriteUTF8String(cell.Skin);
            if (includeName) writer.WriteUTF16String(cell.Name?? "llll");
            else writer.Write<ushort>(0);
        }
        public static void WriteCellData11(Writer writer, Player source, uint protocol, Cell cell, bool includeType, bool includeSize,
            bool includePos, bool includeColor, bool includeName, bool includeSkin)
        {
            writer.Write(cell.id);
            writer.Write((int)cell.X);
            writer.Write((int)cell.Y);
            writer.Write((ushort)cell.Size);

            byte flags = 0;
            if (cell.IsSpiked) 
                flags |= 0x01;
            if (includeColor) 
                flags |= 0x02;
            if (includeSkin) 
                flags |= 0x04;
            if (includeName) 
                flags |= 0x08;
            if (cell.IsAgitated) 
                flags |= 0x10;
            if (cell.Type == 3) 
                flags |= 0x20;
            if (cell.Type == 3 && cell.owner != source) 
                flags |= 0x40;
            if (includeType && cell.Type == 1) 
                flags |= 0x80;
            writer.WriteByte(flags);
            if (includeType && cell.Type == 1)
                writer.WriteByte(1);

            if (includeColor) 
                writer.WriteColor((uint)cell.Color);
            if (includeSkin) 
                writer.WriteUTF8String(cell.Skin);
            if (includeName) 
                writer.WriteUTF8String(cell.Name);
        }
        public static void WriteCellData6(Writer writer, Player source, uint protocol, Cell cell, bool includeType, bool includeSize,
            bool includePos, bool includeColor, bool includeName, bool includeSkin)
        {
            writer.Write(cell.id);
            writer.Write((int)cell.X);
            writer.Write((int)cell.Y);
            writer.Write((ushort)cell.Size);

            byte flags = 0;
            if (cell.IsSpiked) flags |= 0x01;
            if (includeColor) flags |= 0x02;
            if (includeSkin) flags |= 0x04;
            if (includeName) flags |= 0x08;
            if (cell.IsAgitated) flags |= 0x10;
            if (cell.Type == 3) flags |= 0x20;
            writer.WriteByte(flags);
            if (includeColor) writer.WriteColor((uint)cell.Color);
            if (includeSkin) writer.WriteUTF8String(cell.Skin);
            if (includeName) writer.WriteUTF8String(cell.Name);
        }
        private static string ReadZTString(DataReader reader, uint protocol)
        {
            if (protocol < 6)
                return reader.ReadUTF16String();
            else
                return reader.ReadUTF8String(); ;
        }
        private static void WriteZTString(Writer writer, string value, uint protocol)
        {
            if (protocol < 6)
                writer.WriteUTF16String(value);
            else
                writer.WriteUTF8String(value);
        }
    }
}
