using System.Windows.Media.Imaging;

namespace NewMatchingBom.Models
{
    public class ChampionInfo
    {
        public ChampionInfo(string championNameKor, string championNameEng, string championImageUrl, BitmapImage? championImage = null)
        {
            ChampionNameKor = championNameKor;
            ChampionNameEng = championNameEng;
            ChampionImageUrl = championImageUrl;
            ChampionImage = championImage;
        }

        public string ChampionNameKor { get; set; } = string.Empty;
        public string ChampionNameEng { get; set; } = string.Empty;
        public string ChampionImageUrl { get; set; } = string.Empty;
        public BitmapImage? ChampionImage { get; set; }

        public override string ToString()
        {
            return ChampionNameKor;
        }

        public override bool Equals(object? obj)
        {
            if (obj is ChampionInfo other)
            {
                return ChampionNameKor == other.ChampionNameKor;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return ChampionNameKor.GetHashCode();
        }
    }
}