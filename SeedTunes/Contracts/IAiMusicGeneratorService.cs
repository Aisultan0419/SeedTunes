using SeedTunes.Models;

namespace SeedTunes.Contracts
{
    public interface IAiMusicGeneratorService
    {
        Task EnrichWithAiAsync(List<AiMusicMetadata> batch, string languageCode);
    }
}
