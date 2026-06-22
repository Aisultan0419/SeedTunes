using Bogus;
using SeedTunes.Contracts;
using SeedTunes.Models;
using SeedTunes.Infrastructure;
namespace SeedTunes.Services
{
    public class SongService : ISongService
    {
        private readonly ITrackSeedGenerator _trackSeedGenerator;
        private readonly IAiMusicGeneratorService _aiService;
        private readonly IMidiGeneratorService _midiService;
        private readonly ICoverRendererService _coverRendererService;
        public SongService(ITrackSeedGenerator trackSeedGenerator
            ,IAiMusicGeneratorService aiService
            ,IMidiGeneratorService midiService
            ,ICoverRendererService coverRendererService)
        {
            _trackSeedGenerator = trackSeedGenerator;
            _aiService = aiService;
            _midiService = midiService;
            _coverRendererService = coverRendererService;
        }

        public async Task<PageResponse> GetSongsPageAsync(ulong userSeed, int pageNumber, string languageCode, double averageLikes)
        {
            var response = new PageResponse { PageNumber = pageNumber, Songs = new List<SongDto>() };
            var metadataBatch = new List<AiMusicMetadata>();

            int pageSize = 10;
            int startIdx = (pageNumber - 1) * pageSize + 1;

            for (int i = 0; i < pageSize; i++)
            {
                int recordIndex = startIdx + i;
                ulong trackSeed = _trackSeedGenerator.GenerateTrackSeed(userSeed, recordIndex);

                metadataBatch.Add(GenerateDeterministicMetadata(trackSeed, languageCode, recordIndex));
            }

            await _aiService.EnrichWithAiAsync(metadataBatch, languageCode);

            foreach (var m in metadataBatch)
            {
                ulong trackSeed = _trackSeedGenerator.GenerateTrackSeed(userSeed,  m.RecordIndex);
                int likes = CalculateLikes(averageLikes, trackSeed);

                string svgDataUri = await _coverRendererService.RenderCoverAsync(m.CoverPrompt);

                response.Songs.Add(new SongDto
                {
                    Index = m.RecordIndex,
                    Title = m.Title,
                    Artist = m.Artist,
                    Album = m.Album,
                    Genre = m.Genre,
                    CoverUrl = svgDataUri, 
                    AudioUrl = $"/api/songs/audio?userSeed={userSeed}&pageNumber={pageNumber}&songIndex={m.RecordIndex}",
                    LikeCount = likes,
                    Description = m.Description
                });
            }

            return response;
        }

        public byte[] GenerateAudio(ulong userSeed, int pageNumber, int songIndex)
        {
            ulong trackSeed = _trackSeedGenerator.GenerateTrackSeed(userSeed, songIndex);
            var metadata = GenerateDeterministicMetadata(trackSeed, "en", songIndex);
            return _midiService.GenerateMidi(metadata, trackSeed);
        }

        private AiMusicMetadata GenerateDeterministicMetadata(ulong trackSeed, string languageCode, int recordIndex)
        {
            var rng = new Random((int)trackSeed);

            string locale = languageCode.ToLower() == "de" ? "de" : "en";

            int languageSalt = languageCode.ToLower() == "de" ? 2000 : 1000;
            int localizedSeed = (int)trackSeed ^ languageSalt;
            var faker = new Faker(locale) { Random = new Bogus.Randomizer(localizedSeed) };

            var genres = MidiGeneratorService.GenreProfiles.Keys.ToList();
            string chosenGenre = genres[rng.Next(genres.Count)];
            var profile = MidiGeneratorService.GenreProfiles[chosenGenre];

            return new AiMusicMetadata
            {
                RecordIndex = recordIndex,
                TrackSeed = trackSeed,
                Title = faker.Commerce.ProductName(),
                Artist = faker.Name.FullName(),
                Album = faker.Commerce.Color() + " " + faker.Address.City(),
                Genre = chosenGenre,
                Bpm = rng.Next(profile.BpmMin, profile.BpmMax + 1),
                ChordProgression = profile.TypicalProgression[rng.Next(profile.TypicalProgression.Length)],
                Description = faker.Company.CatchPhrase()
            };
        }

        private int CalculateLikes(double averageLikes, ulong trackSeed)
        {
            int safeSeed = (int)(trackSeed & 0x7FFFFFFF);
            var rng = new Random(safeSeed);
            int baseLikes = (int)Math.Floor(averageLikes);
            double fraction = averageLikes - baseLikes;
            if (rng.NextDouble() < fraction) baseLikes++;
            return baseLikes;
        }
    }
}