using Ogar_CSharp.Protocols;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Ogar_CSharp.Other
{
    public abstract class LeaderBoardEntry 
    {
        public abstract void Serialize(Writer writer);
    }
    public class FFALeaderboardEntry : LeaderBoardEntry
    {
        public FFALeaderboardEntry(string name, bool highlighted, uint cellId, short position)
        {
            this.name = name;
            this.highlighted = highlighted;
            this.cellId = cellId;
            this.position = position;
        }
        public string name;
        public bool highlighted;
        public uint cellId;
        public short position;
        public override void Serialize(Writer writer)
        {
            bool hasName = string.IsNullOrEmpty(name);
            if (hasName)
            {
                byte[] nameData = Encoding.UTF8.GetBytes(name);
                writer.WriteBytes(BitConverter.GetBytes((ushort)nameData.Length));
                writer.WriteBytes(nameData);
            }
            writer.Write((uint)cellId);
            writer.WriteByte((byte)(highlighted ? 1 : 0));
            writer.Write((ushort)position);
        }
    }
    public class TextLeaderBoardEntry : LeaderBoardEntry
    {
        public TextLeaderBoardEntry(string text)
        {
            this.text = text;
        }
        public string text;
        public override void Serialize(Writer writer)
        {
            bool hasText = string.IsNullOrEmpty(text);
            if (hasText)
            {
                byte[] textData = Encoding.UTF8.GetBytes(text);
                writer.WriteBytes(BitConverter.GetBytes((ushort)textData.Length));
                writer.WriteBytes(textData);
            }
        }
    }
    public class PieLeaderboardEntry : LeaderBoardEntry
    {
        public PieLeaderboardEntry(float weight, int color)
        {
            this.weight = weight;
            this.color = color;
        }
        public float weight;
        public int color;
        public override void Serialize(Writer writer)
        {
            writer.Write(weight);
            writer.WriteColor((ushort)color);
        }
    }
    public enum LeaderboardType : byte
    {
        FFA,
        Pie,
        Text
    }
}
