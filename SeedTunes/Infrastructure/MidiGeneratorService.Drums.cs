using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using SeedTunes.Enums;
using System;
using System.IO;
using System.Linq;

namespace SeedTunes.Infrastructure
{
    public partial class MidiGeneratorService
    {
        private byte[]? LoadExternalDrumPattern(string genre, int seed)
        {
            string folder = Path.Combine(_patternsPath, GenreToFolder(genre), "drums");
            if (!Directory.Exists(folder)) return null;

            var allFiles = Directory.GetFiles(folder, "*.mid").OrderBy(f => f).ToArray();
            if (allFiles.Length == 0) return null;

            var mainFiles = allFiles
                .Where(f =>
                {
                    string name = Path.GetFileName(f).ToLowerInvariant();
                    return !name.Contains("fill") && !name.Contains("intro") && !name.Contains("outro");
                })
                .ToArray();

            if (mainFiles.Length == 0) mainFiles = allFiles;

            int index = Math.Abs(seed) % mainFiles.Length;
            return File.ReadAllBytes(mainFiles[index]);
        }

        private TrackChunk BuildDrumTrackFromPatternFile(byte[] patternFileBytes, int totalBars, int introBarsToSkip)
        {
            const int SONG_TPQ = 96;
            const int SONG_BAR_TICKS = 4 * SONG_TPQ;

            using var ms = new MemoryStream(patternFileBytes);
            var patternFile = MidiFile.Read(ms);

            var division = patternFile.TimeDivision as TicksPerQuarterNoteTimeDivision;
            int patternTpq = division?.TicksPerQuarterNote ?? 480;
            int patternBarTicks = 4 * patternTpq;

            var allNotes = patternFile.GetNotes().ToList();
            if (!allNotes.Any()) return new TrackChunk();

            long maxPatternTick = allNotes.Max(n => n.Time + n.Length);
            int patternBars = Math.Max(1, (int)Math.Ceiling((double)maxPatternTick / patternBarTicks));

            var events = new List<(long AbsTick, MidiEvent Evt)>();

            long songTotalTicks = (long)totalBars * SONG_BAR_TICKS;
            long introTicks = (long)introBarsToSkip * SONG_BAR_TICKS;

            long patternLenInSongTicks = patternBars * SONG_BAR_TICKS;

            for (long offset = 0; offset < songTotalTicks; offset += patternLenInSongTicks)
            {
                foreach (var note in allNotes)
                {
                    long timeInPattern = note.Time;
                    long lenInPattern = note.Length;

                    long noteOnTick = offset + (timeInPattern * SONG_TPQ / patternTpq);
                    long noteOffTick = offset + ((timeInPattern + lenInPattern) * SONG_TPQ / patternTpq);

                    if (noteOnTick < introTicks) continue;
                    if (noteOnTick >= songTotalTicks) continue;

                    if (noteOffTick > songTotalTicks) noteOffTick = songTotalTicks;
                    if (noteOffTick <= noteOnTick) noteOffTick = noteOnTick + 1;

                    events.Add((noteOnTick, new NoteOnEvent(note.NoteNumber, note.Velocity) { Channel = (FourBitNumber)9 }));
                    events.Add((noteOffTick, new NoteOffEvent(note.NoteNumber, (SevenBitNumber)0) { Channel = (FourBitNumber)9 }));
                }
            }

            var sortedEvents = events
                .OrderBy(e => e.AbsTick)
                .ThenBy(e => e.Evt is NoteOffEvent ? 0 : 1) 
                .ToList();

            var trackChunk = new TrackChunk();
            long prevTick = 0;

            foreach (var (tick, evt) in sortedEvents)
            {
                evt.DeltaTime = tick - prevTick;
                trackChunk.Events.Add(evt);
                prevTick = tick;
            }

            return trackChunk;
        }

        private void BuildDrumPattern(PatternBuilder pattern, DrumStyle style, Random rng)
        {
            var kick = Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)36);
            var snare = Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)38);
            var hihatClosed = Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)42);
            var hihatOpen = Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)46);

            pattern.SetNoteLength(MusicalTimeSpan.Sixteenth);

            switch (style)
            {
                case DrumStyle.ElectroFour:
                    for (int i = 0; i < 16; i++)
                    {
                        bool isKickBeat = (i == 0 || i == 8);
                        bool isSnareBeat = (i == 4 || i == 12);
                        bool isOpenHat = (i == 6 || i == 14);

                        int vel = (isKickBeat ? 100 : isSnareBeat ? 90 : 50) + rng.Next(-5, 6);
                        pattern.SetVelocity((SevenBitNumber)Math.Clamp(vel, 1, 127));

                        if (isKickBeat) pattern.Note(kick);
                        else if (isSnareBeat) pattern.Note(snare);
                        else if (isOpenHat) pattern.Note(hihatOpen);
                        else if (i % 2 == 1) pattern.Note(hihatClosed);
                        else pattern.StepForward(MusicalTimeSpan.Sixteenth);
                    }
                    break;

                case DrumStyle.SynthwaveRetro:
                    for (int i = 0; i < 16; i++)
                    {
                        bool isKick = (i == 0 || i == 8);
                        bool isSnare = (i == 4 || i == 12);
                        bool isHihat = (i == 2 || i == 6 || i == 10 || i == 14);

                        if (isKick)
                        {
                            pattern.SetVelocity((SevenBitNumber)Math.Clamp(95 + rng.Next(-5, 6), 1, 127));
                            pattern.Note(kick);
                        }
                        else if (isSnare)
                        {
                            pattern.SetVelocity((SevenBitNumber)Math.Clamp(80 + rng.Next(-5, 6), 1, 127));
                            pattern.Note(snare);
                        }
                        else if (isHihat)
                        {
                            pattern.SetVelocity((SevenBitNumber)Math.Clamp(42 + rng.Next(-5, 6), 1, 127));
                            pattern.Note(hihatClosed);
                        }
                        else
                        {
                            pattern.StepForward(MusicalTimeSpan.Sixteenth);
                        }
                    }
                    break;

                case DrumStyle.RockBeat:
                    for (int i = 0; i < 16; i++)
                    {
                        bool isKickBeat = (i == 0 || i == 8 || i == 10);
                        bool isSnareBeat = (i == 4 || i == 12);

                        int vel = 85 + rng.Next(-10, 10);
                        pattern.SetVelocity((SevenBitNumber)Math.Clamp(vel, 1, 127));

                        if (isSnareBeat) pattern.Chord(new[] { hihatClosed, snare });
                        else if (isKickBeat && i % 2 == 0) pattern.Chord(new[] { hihatClosed, kick });
                        else if (isKickBeat) pattern.Note(kick);
                        else if (i % 2 == 0) pattern.Note(hihatClosed);
                        else pattern.StepForward(MusicalTimeSpan.Sixteenth);
                    }
                    break;

                case DrumStyle.Trap:
                    for (int i = 0; i < 16; i++)
                    {
                        bool isKickBeat = (i == 0 || i == 3 || i == 8 || i == 11);
                        bool isSnareBeat = (i == 4 || i == 12);

                        int vel = 75 + rng.Next(-10, 10);
                        pattern.SetVelocity((SevenBitNumber)Math.Clamp(vel, 1, 127));

                        if (isSnareBeat) pattern.Chord(new[] { hihatClosed, snare });
                        else if (isKickBeat) pattern.Chord(new[] { hihatClosed, kick });
                        else if (rng.NextDouble() < 0.65) pattern.Note(hihatClosed);
                        else pattern.StepForward(MusicalTimeSpan.Sixteenth);
                    }
                    break;

                case DrumStyle.Disco:
                    for (int i = 0; i < 16; i++)
                    {
                        bool isKickBeat = (i == 0 || i == 4 || i == 8 || i == 12);
                        bool isSnareBeat = (i == 4 || i == 12);
                        bool isOpenHat = (i == 2 || i == 6 || i == 10 || i == 14);

                        int vel = 90 + rng.Next(-5, 5);
                        pattern.SetVelocity((SevenBitNumber)Math.Clamp(vel, 1, 127));

                        if (isSnareBeat) pattern.Chord(new[] { kick, snare });
                        else if (isKickBeat) pattern.Note(kick);
                        else if (isOpenHat) pattern.Note(hihatOpen);
                        else pattern.StepForward(MusicalTimeSpan.Sixteenth);
                    }
                    break;

                case DrumStyle.Breakbeat:
                    for (int i = 0; i < 16; i++)
                    {
                        bool isKickBeat = (i == 0 || i == 6 || i == 10);
                        bool isSnareBeat = (i == 4 || i == 12 || i == 15);

                        int vel = 85 + rng.Next(-10, 10);
                        pattern.SetVelocity((SevenBitNumber)Math.Clamp(vel, 1, 127));

                        if (isSnareBeat && isKickBeat) pattern.Chord(new[] { hihatClosed, snare, kick });
                        else if (isSnareBeat) pattern.Chord(new[] { hihatClosed, snare });
                        else if (isKickBeat) pattern.Chord(new[] { hihatClosed, kick });
                        else if (i % 2 == 0) pattern.Note(hihatClosed);
                        else pattern.StepForward(MusicalTimeSpan.Sixteenth);
                    }
                    break;

                case DrumStyle.None:
                default:
                    pattern.StepForward(MusicalTimeSpan.Whole);
                    break;
            }
        }

        private void BuildDrumFill(PatternBuilder pattern, Random rng)
        {
            var snare = Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)38);
            var kick = Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)36);
            var hat = Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)42);

            pattern.SetNoteLength(MusicalTimeSpan.Sixteenth);
            for (int f = 0; f < 16; f++)
            {
                if (f < 8 && f % 2 != 0)
                {
                    pattern.StepForward(MusicalTimeSpan.Sixteenth);
                    continue;
                }

                bool addKick = (f == 0 || f == 8);
                double vel = 50 + (f * 1.5) + rng.Next(-3, 4);
                pattern.SetVelocity((SevenBitNumber)Math.Clamp(vel, 1, 127));

                if (addKick) pattern.Chord(new[] { kick, snare });
                else pattern.Note(f % 4 == 3 ? hat : snare);
            }
        }
    }
}