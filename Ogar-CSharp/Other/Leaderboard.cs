using Ogar_CSharp.Protocols;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Ogar_CSharp.Other
{
    public interface ILeaderBoardEntry 
    {
        //public abstract void Serialize(Writer writer);
    }
    public readonly struct FFALeaderboardEntry : ILeaderBoardEntry
    {
        public FFALeaderboardEntry(string name, bool highlighted, uint cellId, short position)
        {
            this.name = name;
            this.highlighted = highlighted;
            this.cellId = cellId;
            this.position = position;
        }
        public readonly string name;
        public readonly bool highlighted;
        public readonly uint cellId;
        public readonly short position;
        public override bool Equals(object obj)
        {
            var other = (FFALeaderboardEntry)obj;
            return other.cellId == cellId;
        }
        public override int GetHashCode()
        {
            return unchecked((int)cellId);
        }
    }
    public readonly struct TextLeaderBoardEntry : ILeaderBoardEntry
    {
        public TextLeaderBoardEntry(string text)
        {
            this.text = text;
        }
        public readonly string text;
    }
    public struct PieLeaderboardEntry : ILeaderBoardEntry
    {
        public PieLeaderboardEntry(float weight, OgarColor color)
        {
            this.weight = weight;
            this.color = color;
        }
        public float weight;
        public OgarColor color;
    }
    public enum LeaderboardType : byte
    {
        FFA,
        Pie,
        Text
    }
}
