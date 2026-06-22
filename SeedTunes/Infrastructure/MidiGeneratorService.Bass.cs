using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Interaction;
using SeedTunes.Enums;
using SeedTunes.Models;
using System;

namespace SeedTunes.Infrastructure
{
    public partial class MidiGeneratorService
    {
        private void BuildBassLine(PatternBuilder pattern, BassStyle style, int rootMidi,
            bool isMinor, Random rng, int baseVelocity, ParsedChord chordData)
        {
            int baseBassNote = rootMidi - 24;
            while (baseBassNote > 45) baseBassNote -= 12;
            while (baseBassNote < 28) baseBassNote += 12;

            switch (style)
            {
                case BassStyle.Walking:
                    var scale = isMinor ? MinorScale : MajorScale;
                    pattern.SetNoteLength(MusicalTimeSpan.Quarter);
                    int pos = 0;
                    for (int q = 0; q < 4; q++)
                    {
                        int noteNum = baseBassNote + scale[pos];
                        int vel = baseVelocity + rng.Next(-5, 6);
                        pattern.SetVelocity((SevenBitNumber)Math.Clamp(vel, 1, 127));
                        pattern.Note(Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)noteNum));

                        int direction = (q < 2) ? 1 : -1;
                        pos = Math.Clamp(pos + direction + rng.Next(0, 2), 0, scale.Length - 1);
                    }
                    break;

                case BassStyle.OctaveJump:
                    pattern.SetNoteLength(MusicalTimeSpan.Eighth);
                    for (int i = 0; i < 8; i++)
                    {
                        bool isOffbeat = (i % 2 != 0);
                        int octNote = baseBassNote + (isOffbeat ? 12 : 0);

                        int vel = (isOffbeat ? baseVelocity - 15 : baseVelocity) + rng.Next(-3, 4);
                        pattern.SetVelocity((SevenBitNumber)Math.Clamp(vel, 1, 127));
                        pattern.Note(Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)octNote));
                    }
                    break;

                case BassStyle.SyncopatedGroove:
                    pattern.SetNoteLength(MusicalTimeSpan.Sixteenth);
                    for (int i = 0; i < 16; i++)
                    {
                        int vel = baseVelocity + rng.Next(-5, 6);
                        pattern.SetVelocity((SevenBitNumber)Math.Clamp(vel, 1, 127));

                        if (i == 0 || i == 3 || i == 8 || i == 14)
                            pattern.Note(Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)baseBassNote));
                        else
                            pattern.StepForward(MusicalTimeSpan.Sixteenth);
                    }
                    break;

                case BassStyle.Drone:
                    pattern.SetNoteLength(MusicalTimeSpan.Whole);
                    int droneVel = baseVelocity + rng.Next(-5, 6);
                    pattern.SetVelocity((SevenBitNumber)Math.Clamp(droneVel, 1, 127));
                    pattern.Note(Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)baseBassNote));
                    break;

                case BassStyle.Arpeggio:
                    pattern.SetNoteLength(MusicalTimeSpan.Quarter);
                    int[] arpIntervals = { 0, 7, 12, 7 };
                    for (int a = 0; a < 4; a++)
                    {
                        int arpNote = baseBassNote + arpIntervals[a];
                        int accent = (a == 0) ? 5 : 0;
                        int vel = baseVelocity + accent + rng.Next(-3, 4);
                        pattern.SetVelocity((SevenBitNumber)Math.Clamp(vel, 1, 127));
                        pattern.Note(Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)arpNote));
                    }
                    break;

                case BassStyle.Pulsing:
                default:
                    pattern.SetNoteLength(MusicalTimeSpan.Eighth);
                    for (int b = 0; b < 8; b++)
                    {
                        int accent = (b % 4 == 0) ? 3 : (b % 2 == 0) ? 0 : -10;
                        int vel = baseVelocity + accent + rng.Next(-3, 4);
                        pattern.SetVelocity((SevenBitNumber)Math.Clamp(vel, 1, 127));
                        pattern.Note(Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)baseBassNote));
                    }
                    break;
            }
        }
    }
}