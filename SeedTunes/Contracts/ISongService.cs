using SeedTunes.Models;

namespace SeedTunes.Contracts
{
    public interface ISongService
    {
        Task<PageResponse> GetSongsPageAsync(ulong userSeed, int pageNumber, string languageCode, double averageLikes);
        byte[] GenerateAudio(ulong userSeed, int pageNumber, int songIndex);
    }
}
