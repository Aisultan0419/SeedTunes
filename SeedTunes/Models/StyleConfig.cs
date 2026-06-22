using SeedTunes.Infrastructure;

namespace SeedTunes.Models
{
    public class StyleConfig
    {
        public List<string>? Palette { get; set; }
        public string? Mood { get; set; }
        public EffectConfig? Effects { get; set; }
    }
}
