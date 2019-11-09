using System;
using System.Collections.Generic;
using System.Text;
using Ogar_CSharp.Cells;
using Ogar_CSharp.Other;
using Ogar_CSharp.Sockets;
using System.Linq;
using Ogar_CSharp.Worlds;
using System.Threading.Tasks;

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
        internal static Protocol Decider(Reader reader, Connection connection)
        {
            if (reader.length < 5)
                return null;
            if (reader.Read<byte>() != 254)
                return null;
            return new LegacyProtocol(connection) { gotProtocol = true, protocolVersion = reader.Read<uint>() };
        }
        public override void OnLeaderboardUpdate(LeaderboardType type, IEnumerable<LeaderBoardEntry> data, LeaderBoardEntry selfData)
        {
            this.LastleaderboardType = type;
            var writer = new Writer();
            switch (type)
            {
                case LeaderboardType.FFA: 
                    FFALeaderboard(writer, data.Cast<FFALeaderboardEntry>().ToList(), (FFALeaderboardEntry)selfData, protocolVersion); 
                    break;
                case LeaderboardType.Pie: 
                    PieLeaderboard(writer, data.Cast<PieLeaderboardEntry>().ToList(), (PieLeaderboardEntry)selfData, protocolVersion); 
                    break;
                case LeaderboardType.Text: 
                    TextBoard(writer, data.Cast<TextLeaderBoardEntry>().ToList(), protocolVersion);
                    break;
            }
            this.Send(writer.RawBuffer);
        }

        public override void OnNewOwnedCell(PlayerCell cell) 
            => Send(new Writer() { (byte)32, (uint)cell.id }.RawBuffer);
   

        public override void OnNewWorldBounds(Rect range, bool includeServerInfo)
        {
            var writer = new Writer() 
            { (byte)64, (double)(range.x - range.w), (double)(range.y - range.h), (double)(range.x + range.w), (double)(range.y + range.h) };
            if (includeServerInfo)
            {
                writer.WriteUInt(Handle.gamemode.Type);
                WriteZTString(writer, $"OgarII-CSharp {Handle.Version}", protocolVersion);
            }
            this.Send(writer.RawBuffer);
        }

        public override void OnSocketMessage(Reader reader)
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
            => Send(new Writer() { (byte)17, area.x, area.y, area.s }.RawBuffer);
        public static void WriteCellData(Writer writer, Player source, uint protocol, Cell cell, bool includeType, bool includeSize,
            bool includePos, bool includeColor, bool includeName, bool includeSkin)
        {
            if (protocol == 4 || protocol == 5)
                WriteCellData4(writer, source, protocol, cell, includeType, includeSize, includePos, includeColor, includeName, includeSkin);
            else if (protocol <= 10)
                WriteCellData6(writer, source, protocol, cell, includeType, includeSize, includePos, includeColor, includeName, includeSkin);
            else if (protocol <= 21)
                WriteCellData11(writer, source, protocol, cell, includeType, includeSize, includePos, includeColor, includeName, includeSkin);
        }
        public override void OnVisibleCellUpdate(IEnumerable<Cell> add, IEnumerable<Cell> upd, IEnumerable<Cell> eat, IEnumerable<Cell> del)
        {
            var source = this.connection.Player;
            var writer = new Writer() { (byte)16, (ushort)eat.Count() };
            foreach (var item in eat)
            {
                writer.WriteUInt((uint)item.eatenBy.id);
                writer.WriteUInt((uint)item.id);
            }
            foreach (var item in add)
            {
                WriteCellData(writer, source, protocolVersion, item,
                    true, true, true, true, true, true);
            }
            foreach (var item in upd)
            {
                WriteCellData(writer, source, protocolVersion, item,
                    false, item.sizeChanged, item.posChanged, item.colorChanged, item.nameChanged, item.skinChanged);
            }
            writer.WriteUInt(0);
            if (protocolVersion < 6)
                writer.WriteUInt((uint)del.Count());
            else
                writer.WriteUShort((ushort)del.Count());
            foreach (var item in del)
                writer.WriteUInt((uint)item.id);
            this.Send(writer.RawBuffer);
        }

        public override void OnWorldReset()
        {
            this.Send(new Writer() { (byte)18 }.RawBuffer);
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
                var writer = new Writer() { (byte)254 };
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
                    Send(writer.RawBuffer);
                    isCompilingStats = false;
                });
            }
        }
        public static void PieLeaderboard(Writer writer, List<PieLeaderboardEntry> data, PieLeaderboardEntry selfData, uint protocol)
        {
            if (protocol <= 20)
                PieLeaderboard4(writer, data, selfData, protocol);
            else if(protocol == 21)
                PieLeaderboard21(writer, data, selfData, protocol);
        }
        public static void PieLeaderboard4(Writer writer, List<PieLeaderboardEntry> data, PieLeaderboardEntry selfData, uint protocol)
        {
            writer.WriteByte(50);
            writer.WriteUInt((uint)data.Count);
            for (int i = 0, l = data.Count; i < l; i++)
                writer.WriteFloat(data[i].weight);
        }
        public static void PieLeaderboard21(Writer writer, List<PieLeaderboardEntry> data, PieLeaderboardEntry selfData, uint protocol)
        {
            writer.WriteByte(51);
            writer.WriteUInt((uint)data.Count);
            for (int i = 0, l = data.Count; i < l; i++)
            {
                writer.WriteFloat(data[i].weight);
                writer.WriteColor((uint)data[i].color);
            }
        }
        public static void TextBoard(Writer writer, List<TextLeaderBoardEntry> data, uint protocol)
        {
            if (protocol <= 13)
                TextBoard4(writer, data, protocol);
            else if (protocol <= 21)
                TextBoard14(writer, data, protocol);
        }
        public static void TextBoard4(Writer writer, List<TextLeaderBoardEntry> data, uint protocol)
        {
            writer.WriteByte(48);
            writer.WriteUInt((uint)data.Count);
            for (int i = 0, l = data.Count; i < l; i++)
                WriteZTString(writer, data[i].text, protocol);
        }
        public static void TextBoard14(Writer writer, List<TextLeaderBoardEntry> data, uint protocol)
        {
            writer.WriteByte(53);
            writer.WriteUInt((uint)data.Count);
            for (int i = 0, l = data.Count; i < l; i++)
            {
                writer.WriteByte(2);
                WriteZTString(writer, data[i].text, protocol);
            }
        }
        public static void FFALeaderboard(Writer writer, List<FFALeaderboardEntry> data, FFALeaderboardEntry selfdata, uint protocol)
        {
            if (protocol <= 10)
                FFALeaderBoard4(writer, data, selfdata, protocol);
            else if (protocol <= 21)
                FFALeaderBoard11(writer, data, selfdata, protocol);
        }
        public static void FFALeaderBoard4(Writer writer, List<FFALeaderboardEntry> data, FFALeaderboardEntry selfdata, uint protocol)
        {
            writer.WriteByte(49);
            writer.WriteUInt((uint)data.Count);
            for (int i = 0, l = data.Count; i < l; i++)
            {
                var item = data[i];
                if (protocol == 6)
                    writer.WriteUInt((uint)(item.highlighted ? 1 : 0));
                else writer.WriteUInt((uint)item.cellId);
                WriteZTString(writer, item.name, protocol);
            }
        }
        public static void FFALeaderBoard11(Writer writer, List<FFALeaderboardEntry> data, FFALeaderboardEntry selfdata, uint protocol)
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
            writer.WriteUInt((uint)cell.id);
            if(protocol == 4)
            {
                writer.WriteShort((short)cell.X);
                writer.WriteShort((short)cell.Y);
            }
            else
            {
                writer.WriteInt((int)cell.X);
                writer.WriteInt((int)cell.Y);
            }

            writer.WriteUShort((ushort)cell.Size);
            writer.WriteColor((uint)cell.Color);

            byte flags = 0;
            if (cell.IsSpiked) flags |= 0x01;
            if (includeSkin) flags |= 0x04;
            if (cell.IsAgitated) flags |= 0x10;
            if (cell.Type == 3) flags |= 0x20;
            writer.WriteByte(flags);

            if (includeSkin) writer.WriteUTF8String(cell.Skin);
            if (includeName) writer.WriteUTF16String(cell.Name);
            else writer.WriteUShort(0);
        }
        public static void WriteCellData11(Writer writer, Player source, uint protocol, Cell cell, bool includeType, bool includeSize, 
            bool includePos, bool includeColor, bool includeName, bool includeSkin)
        {
            writer.WriteUInt((uint)cell.id);
            writer.WriteUInt((uint)cell.Y);
            writer.WriteUInt((uint)cell.Y);
            writer.WriteUShort((ushort)cell.Size);

            byte flags = 0;
            if (cell.IsSpiked) flags |= 0x01;
            if (includeColor) flags |= 0x02;
            if (includeSkin) flags |= 0x04;
            if (includeName) flags |= 0x08;
            if (cell.IsAgitated) flags |= 0x10;
            if (cell.Type == 3) flags |= 0x20;
            if (cell.Type == 3 && cell.owner != source) flags |= 0x40;
            if (includeType && cell.Type == 1) flags |= 0x80;
            writer.WriteByte(flags);
            if (includeType && cell.Type == 1) writer.WriteByte(1);

            if (includeColor) writer.WriteColor((uint)cell.Color);
            if (includeSkin) writer.WriteUTF8String(cell.Skin);
            if (includeName) writer.WriteUTF8String(cell.Name);
        }
        public static void WriteCellData6(Writer writer, Player source, uint protocol, Cell cell, bool includeType, bool includeSize,
            bool includePos, bool includeColor, bool includeName, bool includeSkin)
        {
            writer.WriteUInt((uint)cell.id);
            writer.WriteInt((int)cell.X);
            writer.WriteInt((int)cell.Y);
            writer.WriteUShort((ushort)cell.Size);

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
        private static string ReadZTString(Reader reader, uint protocol)
        {
            if (protocol < 6)
                return reader.Read<string>(null, true);
            else
                return reader.Read<string>(null, false);
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
