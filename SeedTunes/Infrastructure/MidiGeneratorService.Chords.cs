using SeedTunes.Models;
using System;
using System.Collections.Generic;

namespace SeedTunes.Infrastructure
{
    public partial class MidiGeneratorService
    {
        private int ParseRootMidi(string chord)
        {
            chord = chord.Trim();
            string root = (chord.Length >= 2 && (chord[1] == '#' || chord[1] == 'b'))
                ? chord.Substring(0, 2)
                : chord.Substring(0, 1);

            root = root switch
            {
                "Bb" => "A#",
                "Eb" => "D#",
                "Ab" => "G#",
                "Db" => "C#",
                "Gb" => "F#",
                "Cb" => "B",
                _ => root
            };

            int idx = Array.IndexOf(_notes, root);
            return idx == -1 ? 60 : 48 + idx;
        }

        private bool ParseIsMinor(string chord)
        {
            int skip = (chord.Length >= 2 && (chord[1] == '#' || chord[1] == 'b')) ? 2 : 1;
            if (chord.Length <= skip) return false;
            string suffix = chord.Substring(skip).ToLower();
            return suffix.StartsWith("m") && !suffix.StartsWith("maj");
        }

        private List<ParsedChord> ParseChordProgressionAdvanced(string progression)
        {
            var result = new List<ParsedChord>();
            var chords = progression.Split('-', StringSplitOptions.RemoveEmptyEntries);

            if (chords.Length == 0) chords = new[] { "C", "G", "Am", "F" };

            foreach (var c in chords)
            {
                var clean = c.Trim();
                int root = ParseRootMidi(clean);
                bool minor = ParseIsMinor(clean);

                while (root < 48) root += 12;
                while (root > 59) root -= 12;

                int third = root + (minor ? 3 : 4);
                int fifth = root + 7;

                bool hasMaj7 = clean.Contains("maj7", StringComparison.OrdinalIgnoreCase);
                bool hasMin7 = !hasMaj7 && clean.Contains("7");
                int seventh = hasMaj7 ? root + 11 : hasMin7 ? root + 10 : -1;

                if (seventh != -1)
                {
                    while (seventh > 72) seventh -= 12;
                    while (seventh < 48) seventh += 12;
                }

                result.Add(new ParsedChord
                {
                    RootMidi = root,
                    ThirdMidi = third,
                    FifthMidi = fifth,
                    SeventhMidi = seventh,
                    IsMinor = minor
                });
            }
            return result;
        }
    }
}