namespace SeedTunes.Contracts
{
    public interface ITrackSeedGenerator
    {
        ulong GenerateTrackSeed(ulong userSeed, int trackIndex);
    }
}
