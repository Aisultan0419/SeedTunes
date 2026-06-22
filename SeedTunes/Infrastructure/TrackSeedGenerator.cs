using SeedTunes.Contracts;
using System.IO.Hashing;
using System.Runtime.InteropServices;
namespace SeedTunes.Infrastructure
{
    public class TrackSeedGenerator : ITrackSeedGenerator
    {
        public ulong GenerateTrackSeed(ulong userSeed, int trackIndex)
        {
            ulong finalCoordinates = userSeed ^ (ulong)trackIndex;
            Span<byte> buffer = stackalloc byte[sizeof(ulong)];
            MemoryMarshal.Write(buffer, in finalCoordinates);
            byte[] hashBytes = XxHash64.Hash(buffer);
            return BitConverter.ToUInt64(hashBytes, 0);
        }
    }
}
