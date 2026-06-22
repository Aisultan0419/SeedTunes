using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Interaction;
using SeedTunes.Models;
using System;

namespace SeedTunes.Infrastructure
{
    public partial class MidiGeneratorService
    {
        private void BuildMelodyPhrase(PatternBuilder pattern, int keyRoot, bool isMinor,
             Random rng, bool isChorus, float density, int maxStep, int velocityBase,
             ref int scalePos, ParsedChord currentChord)
        {
            int[][] humanMotifs = new[]
            {
                new[] { 0, 1, 2, 3, 2, 1, 0, 1 },
                new[] { 0, 0, 2, 2, 3, 2, 1, 0 },
                new[] { 0, 2, 1, 2, 3, 1, 2, 0 },
                new[] { 3, 2, 1, 2, 0, 1, 2, 3 }
            };

            int motifIdx = Math.Abs(currentChord.RootMidi + scalePos) % humanMotifs.Length;
            int[] motif = humanMotifs[motifIdx];

            int baseNote = currentChord.RootMidi + 24 + (isChorus ? 12 : 0);
            while (baseNote > 80) baseNote -= 12;
            while (baseNote < 60) baseNote += 12;

            int[] chordTones = {
                baseNote,
                baseNote + (currentChord.ThirdMidi - currentChord.RootMidi),
                baseNote + (currentChord.FifthMidi - currentChord.RootMidi),
                baseNote + 12
            };

            const int BAR_SIXTEENTHS = 16;
            int elapsed = 0;

            pattern.SetNoteLength(MusicalTimeSpan.Eighth);

            while (elapsed < BAR_SIXTEENTHS)
            {
                int stepIndex = (elapsed / 2) % 8;

                if (elapsed > 0 && stepIndex % 2 != 0 && rng.NextDouble() > density)
                {
                    pattern.StepForward(MusicalTimeSpan.Eighth);
                    elapsed += 2;
                    continue;
                }

                int toneIndex = Math.Clamp(motif[stepIndex], 0, 3);
                int raw = chordTones[toneIndex];

                bool isStrongBeat = (elapsed % 4 == 0);
                int accentBonus = isStrongBeat ? 8 : -4;
                int velocity = velocityBase + accentBonus + rng.Next(-3, 4);

                if (raw > 76) velocity = (int)(velocity * 0.75);

                pattern.SetVelocity((SevenBitNumber)Math.Clamp(velocity, 1, 127));
                pattern.Note(Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)raw));

                elapsed += 2;
            }

            scalePos = (scalePos + 1) % 4;
        }
    }
}