using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Standards;
using SeedTunes.Models;
using SeedTunes.Contracts;
using SeedTunes.Enums;

namespace SeedTunes.Infrastructure
{
    public partial class MidiGeneratorService : IMidiGeneratorService
    {
        private readonly string _patternsPath;
        private readonly string[] _notes = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
        private static readonly int[] MajorScale = { 0, 2, 4, 5, 7, 9, 11, 12 };
        private static readonly int[] MinorScale = { 0, 2, 3, 5, 7, 8, 10, 12 };

        public MidiGeneratorService(IWebHostEnvironment env)
        {
            _patternsPath = Path.Combine(env.ContentRootPath, "MidiPatterns");
        }

        private static string GenreToFolder(string genre) => genre switch
        {
            "Trap / Hip-Hop" => "trap",
            "Lo-Fi" => "lofi",
            "EDM / House" => "house",
            "Synthwave" => "synthwave",
            "Rock / Metal" => "rock",
            "Disco / Pop" => "disco",
            _ => "trap"
        };

        public byte[] GenerateMidi(AiMusicMetadata metadata, ulong trackSeed)
        {
            var midiFile = new MidiFile();
            var tempoMap = TempoMap.Create(Tempo.FromBeatsPerMinute(metadata.Bpm));
            midiFile.ReplaceTempoMap(tempoMap);

            int safeSeed = (int)(trackSeed & 0x7FFFFFFF);
            var rng = new Random(safeSeed);

            if (!GenreProfiles.TryGetValue(metadata.Genre, out var profile))
                profile = GenreProfiles["Trap / Hip-Hop"];

            var chordsPattern = new PatternBuilder();
            var bassPattern = new PatternBuilder();
            var leadPattern = new PatternBuilder();
            var drumPattern = new PatternBuilder();

            var parsedChords = ParseChordProgressionAdvanced(metadata.ChordProgression);
            int sections = 4;

            int padProg = profile.PadPrograms[rng.Next(profile.PadPrograms.Length)];
            int leadProg = profile.LeadPrograms[rng.Next(profile.LeadPrograms.Length)];
            int bassProg = profile.BassPrograms[rng.Next(profile.BassPrograms.Length)];

            int keyRoot = parsedChords[0].RootMidi;
            bool keyIsMinor = parsedChords[0].IsMinor;

            int totalChunks = 2 * parsedChords.Count;
            int melodyScalePos = rng.Next(4);

            int totalBars = sections * 2 * parsedChords.Count;
            int introBars = Math.Max(1, parsedChords.Count / 2);
            byte[]? externalDrums = LoadExternalDrumPattern(metadata.Genre, safeSeed);

            for (int sectionIndex = 0; sectionIndex < sections; sectionIndex++)
            {
                bool isChorus = (sectionIndex % 2 != 0);

                for (int repeat = 0; repeat < 2; repeat++)
                {
                    for (int chordIndex = 0; chordIndex < parsedChords.Count; chordIndex++)
                    {
                        var chordData = parsedChords[chordIndex];
                        int rootMidi = chordData.RootMidi;

                        float progress = (float)(repeat * parsedChords.Count + chordIndex) / totalChunks;
                        int sectionBase = isChorus ? 62 : 45;
                        int buildBonus = (int)(progress * 12);

                        int chordVel = Math.Max(sectionBase + buildBonus - 12, 40);
                        int bassVel = sectionBase + buildBonus;
                        int melodyVelBase = sectionBase + buildBonus + 6;

                        bool isIntro = (sectionIndex == 0 && repeat == 0);
                        bool isIntroFirstHalf = isIntro && (chordIndex < introBars);

                        chordsPattern.SetVelocity((SevenBitNumber)Math.Clamp(chordVel, 1, 127));
                        var chordNotes = new List<Melanchall.DryWetMidi.MusicTheory.Note>
                        {
                            Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)rootMidi),
                            Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)chordData.ThirdMidi),
                            Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)chordData.FifthMidi)
                        };
                        if (chordData.SeventhMidi != -1)
                            chordNotes.Add(Melanchall.DryWetMidi.MusicTheory.Note.Get((SevenBitNumber)chordData.SeventhMidi));

                        if (!isChorus && !isIntroFirstHalf)
                        {
                            chordsPattern.SetNoteLength(MusicalTimeSpan.Half);
                            chordsPattern.Chord(chordNotes.ToArray());
                            chordsPattern.Chord(chordNotes.ToArray());
                        }
                        else
                        {
                            chordsPattern.SetNoteLength(MusicalTimeSpan.Whole);
                            chordsPattern.Chord(chordNotes.ToArray());
                        }

                        if (isIntroFirstHalf)
                            bassPattern.StepForward(MusicalTimeSpan.Whole);
                        else
                            BuildBassLine(bassPattern, profile.BassStyle, rootMidi, chordData.IsMinor, rng, bassVel, chordData);

                        int localMelodyVel = melodyVelBase;
                        if (sectionIndex == 0 && repeat == 1 && chordIndex == 0)
                            localMelodyVel -= 15;

                        if (isIntro)
                            leadPattern.StepForward(MusicalTimeSpan.Whole);
                        else
                            BuildMelodyPhrase(leadPattern, keyRoot, keyIsMinor, rng,
                                isChorus, profile.MelodyDensity, profile.MaxMelodyStep,
                                localMelodyVel, ref melodyScalePos, chordData);

                        if (externalDrums == null)
                        {
                            bool isPreChorus = (sectionIndex == 0 || sectionIndex == 2);
                            bool isLastChord = (repeat == 1 && chordIndex == parsedChords.Count - 1);

                            if (isPreChorus && isLastChord && profile.AggressiveFills && profile.DrumStyle != DrumStyle.None)
                                BuildDrumFill(drumPattern, rng);
                            else if (isIntroFirstHalf)
                                drumPattern.StepForward(MusicalTimeSpan.Whole);
                            else
                                BuildDrumPattern(drumPattern, profile.DrumStyle, rng);
                        }
                    }
                }
            }

            var chordsTrack = chordsPattern.Build().ToTrackChunk(tempoMap);
            var bassTrack = bassPattern.Build().ToTrackChunk(tempoMap);
            var leadTrack = leadPattern.Build().ToTrackChunk(tempoMap);

            TrackChunk drumTrack;
            if (externalDrums != null)
                drumTrack = BuildDrumTrackFromPatternFile(externalDrums, totalBars, introBars);
            else
                drumTrack = drumPattern.Build().ToTrackChunk(tempoMap);

            AssignInstrument(chordsTrack, (GeneralMidiProgram)padProg, (FourBitNumber)0);
            AssignInstrument(bassTrack, (GeneralMidiProgram)bassProg, (FourBitNumber)1);
            AssignInstrument(leadTrack, (GeneralMidiProgram)leadProg, (FourBitNumber)2);
            AssignInstrument(drumTrack, (GeneralMidiProgram)0, (FourBitNumber)9);

            midiFile.Chunks.Add(chordsTrack);
            midiFile.Chunks.Add(bassTrack);
            midiFile.Chunks.Add(leadTrack);
            midiFile.Chunks.Add(drumTrack);

            using var memoryStream = new MemoryStream();
            midiFile.Write(memoryStream);
            return memoryStream.ToArray();
        }

        private void AssignInstrument(TrackChunk track, GeneralMidiProgram program, FourBitNumber channel)
        {
            track.Events.Insert(0, new ProgramChangeEvent((SevenBitNumber)(byte)program) { Channel = channel });

            foreach (var midiEvent in track.Events)
            {
                if (midiEvent is ChannelEvent channelEvent)
                    channelEvent.Channel = channel;
            }
        }
    }
}