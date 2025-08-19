using System;

namespace NewMatchingBom.Models
{
    public class PlayRecord
    {
        public string Name { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Point { get; set; } = 0;

        public override string ToString()
        {
            return $"{Name} - {Date} - {Type} - {Point}점";
        }
    }

    public class RecordViewData
    {
        public string Name { get; set; } = string.Empty;
        public int Rank { get; set; } = 0;
        public int TotalPoint { get; set; } = 0;
        public override string ToString()
        {
            return $"{Rank}위. {Name} - {TotalPoint}점";
        }
    }
}