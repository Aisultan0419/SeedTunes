using SeedTunes.Models;

namespace SeedTunes.Contracts
{
    public interface IMidiGeneratorService
    {
        byte[] GenerateMidi(AiMusicMetadata metadata, ulong trackSeed);
    }
}
