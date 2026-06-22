using System.Text.Json.Serialization;

namespace SeedTunes.Models
{
    public class AiMusicMetadata
    {
        [JsonIgnore]
        public int RecordIndex { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("artist")]
        public string Artist { get; set; } = string.Empty;

        [JsonPropertyName("album")]
        public string Album { get; set; } = string.Empty;

        [JsonPropertyName("genre")]
        public string Genre { get; set; } = string.Empty;

        [JsonPropertyName("bpm")]
        public int Bpm { get; set; }

        [JsonPropertyName("chordProgression")]
        public string ChordProgression { get; set; } = string.Empty;

        [JsonPropertyName("coverPrompt")]
        public string CoverPrompt { get; set; } = string.Empty;
        public string TimeSignature { get; set; } = "4/4";
        public string Key { get; set; } = "C Major";
        public string LeadInstrument { get; set; } = "Synth";
        public string BassInstrument { get; set; } = "Bass";
        public string RhythmFeel { get; set; } = "Straight";
        public string Structure { get; set; } = "Verse-Chorus";
        public string MelodyContour { get; set; } = "Wave";
        public ulong TrackSeed { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}