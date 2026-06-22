using SeedTunes.Infrastructure;

namespace SeedTunes.Models
{
    public class CoverPromptData
    {
        public long? TrackSeed { get; set; }
        public StyleConfig? Style { get; set; }
        public AssetConfig? BackgroundShape { get; set; }
        public AssetConfig? HeroAsset { get; set; }
        public string? Artist { get; set; }
        public string? Album { get; set; }
    }
}
