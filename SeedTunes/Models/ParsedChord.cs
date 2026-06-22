namespace SeedTunes.Models
{
    public class ParsedChord
    {
        public int RootMidi { get; set; }
        public int ThirdMidi { get; set; }
        public int FifthMidi { get; set; }
        public int SeventhMidi { get; set; } = -1;
        public bool IsMinor { get; set; }
    }
}