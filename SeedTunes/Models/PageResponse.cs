namespace SeedTunes.Models
{
    public class PageResponse
    {
        public int PageNumber { get; set; }
        public List<SongDto> Songs { get; set; } = new();
    }
}
