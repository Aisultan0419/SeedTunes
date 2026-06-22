using SeedTunes.Enums;
using SeedTunes.Models;
using System.Collections.Generic;

namespace SeedTunes.Infrastructure
{
    public partial class MidiGeneratorService
    {
        public static readonly Dictionary<string, GenreProfile> GenreProfiles = new()
        {
            ["Trap / Hip-Hop"] = new GenreProfile
            {
                BpmMin = 110,
                BpmMax = 145,
                DrumStyle = DrumStyle.Trap,
                BassStyle = BassStyle.SyncopatedGroove,
                MelodyDensity = 0.40f,
                TypicalProgression = new[] { "Cm - Fm - G - Cm", "Dm - Gm - A - Dm", "Am - Em - F - E", "Gm - Cm - D - Gm", "Fm - Cm - Ab - Bb", "Em - Am - B - Em", "Bbm - Fm - Gb - Ab", "Cm - Ab - Bb - G" },
                LeadPrograms = new[] { 80, 5, 87 },
                BassPrograms = new[] { 39, 38 },
                PadPrograms = new[] { 89, 91 },
                MaxMelodyStep = 2,
                AggressiveFills = true
            },
            ["Lo-Fi"] = new GenreProfile
            {
                BpmMin = 70,
                BpmMax = 90,
                DrumStyle = DrumStyle.Breakbeat,
                BassStyle = BassStyle.Walking,
                MelodyDensity = 0.38f,
                TypicalProgression = new[] { "Am7 - Dm7 - G7 - Cmaj7", "Fmaj7 - Em7 - Dm7 - Cmaj7", "Dm7 - G7 - Em7 - Am7", "Cmaj7 - Am7 - Fmaj7 - G7", "Em7 - Am7 - Dm7 - G7", "Bbmaj7 - Am7 - Gm7 - Fmaj7", "Dm7 - Em7 - Fmaj7 - G7", "Am7 - Fmaj7 - Cmaj7 - G7" },
                LeadPrograms = new[] { 4, 5, 73 },
                BassPrograms = new[] { 32, 33 },
                PadPrograms = new[] { 88, 4 },
                MaxMelodyStep = 2,
                AggressiveFills = false
            },
            ["EDM / House"] = new GenreProfile
            {
                BpmMin = 120,
                BpmMax = 130,
                DrumStyle = DrumStyle.ElectroFour,
                BassStyle = BassStyle.Pulsing,
                MelodyDensity = 0.50f,
                TypicalProgression = new[] { "Am - F - C - G", "Em - C - G - D", "Dm - Am - Em - G", "Fm - Cm - Ab - Bb", "Am - Dm - F - G", "Cm - Ab - Eb - Bb", "Em - Am - C - D", "F - G - Am - C" },
                LeadPrograms = new[] { 81, 80, 90 },
                BassPrograms = new[] { 38, 39 },
                PadPrograms = new[] { 89, 92 },
                MaxMelodyStep = 3,
                AggressiveFills = true
            },
            ["Synthwave"] = new GenreProfile
            {
                BpmMin = 100,
                BpmMax = 125,
                DrumStyle = DrumStyle.SynthwaveRetro,
                BassStyle = BassStyle.Arpeggio,
                MelodyDensity = 0.50f,
                TypicalProgression = new[] { "Am - F - C - G", "Dm - Bb - F - C", "Em - C - D - Am", "Fm - Db - Ab - Eb", "Am - Em - F - Dm", "Cm - Ab - Eb - Bb", "Gm - Eb - Bb - F", "Am - Dm - G - C" },
                LeadPrograms = new[] { 81, 82, 88, 89 },
                BassPrograms = new[] { 38, 39 },
                PadPrograms = new[] { 89, 88 },
                MaxMelodyStep = 3,
                AggressiveFills = false
            },
            ["Rock / Metal"] = new GenreProfile
            {
                BpmMin = 130,
                BpmMax = 180,
                DrumStyle = DrumStyle.RockBeat,
                BassStyle = BassStyle.Pulsing,
                MelodyDensity = 0.55f,
                TypicalProgression = new[] { "Em - C - G - D", "Am - F - C - G", "Dm - Bb - F - C", "Em - G - D - Am", "Am - C - G - F", "Bm - G - D - A", "Em - Am - D - G", "Am - Dm - Em - Am" },
                LeadPrograms = new[] { 30, 29 },
                BassPrograms = new[] { 34, 33 },
                PadPrograms = new[] { 48, 49 },
                MaxMelodyStep = 4,
                AggressiveFills = true
            },
            ["Disco / Pop"] = new GenreProfile
            {
                BpmMin = 110,
                BpmMax = 130,
                DrumStyle = DrumStyle.Disco,
                BassStyle = BassStyle.OctaveJump,
                MelodyDensity = 0.50f,
                TypicalProgression = new[] { "Am7 - D7 - Gmaj7 - E7", "Dm7 - G7 - Cmaj7 - A7", "Fm7 - Bb7 - Ebmaj7 - C7", "Em7 - A7 - Dmaj7 - B7", "Cm7 - F7 - Bbmaj7 - G7", "Gm7 - C7 - Fmaj7 - D7", "Am7 - Dm7 - G7 - C7", "Dm7 - Em7 - Fmaj7 - G7" },
                LeadPrograms = new[] { 28, 5, 61 },
                BassPrograms = new[] { 36, 37 },
                PadPrograms = new[] { 4, 88 },
                MaxMelodyStep = 3,
                AggressiveFills = false
            }
        };
    }
}