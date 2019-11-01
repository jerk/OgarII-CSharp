using Ogar_CSharp.protocols;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ogar_CSharp.Other
{
    public abstract class LeaderBoardEntry { }
    public class FFALeaderboardEntry : LeaderBoardEntry
    {
        public FFALeaderboardEntry(string name, bool highlighted, int cellId, short position)
        {
            this.name = name;
            this.highlighted = highlighted;
            this.cellId = cellId;
            this.position = position;
        }
        public string name;
        public bool highlighted;
        public int cellId;
        public short position;
    }
    public class TextLeaderBoardEntry : LeaderBoardEntry
    {
        public TextLeaderBoardEntry(string text)
        {
            this.text = text;
        }
        public string text;
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
    }
    public enum LeaderboardType
    {
        FFA,
        Pie,
        Text
    }
}
