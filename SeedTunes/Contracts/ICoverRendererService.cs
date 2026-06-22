namespace SeedTunes.Contracts
{
    public interface ICoverRendererService
    {
        Task<string> RenderCoverAsync(string jsonPrompt);
    }
}
