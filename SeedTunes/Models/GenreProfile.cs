using SeedTunes.Enums;

namespace SeedTunes.Models
{
    public class GenreProfile
    {
        public int BpmMin { get; set; }
        public int BpmMax { get; set; }
        public DrumStyle DrumStyle { get; set; }
        public BassStyle BassStyle { get; set; }
        public float MelodyDensity { get; set; }
        public string[] TypicalProgression { get; set; } = [];
        public int[] LeadPrograms { get; set; } = [];
        public int[] BassPrograms { get; set; } = [];
        public int[] PadPrograms { get; set; } = [];
        public int MaxMelodyStep { get; set; } = 3;
        public bool AggressiveFills { get; set; } = true;
    }
}