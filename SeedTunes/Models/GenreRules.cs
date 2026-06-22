using SeedTunes.Infrastructure;

namespace SeedTunes.Models
{
    public class GenreRules
    {
        public List<List<string>> Palettes { get; set; }
        public List<string> BackgroundShapes { get; set; } 
        public List<string> HeroAssets { get; set; }      
        public EffectsConfig Effects { get; set; }
    }
}
