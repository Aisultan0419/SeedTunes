namespace SeedTunes.Models
{
    public class SongDto
    {
        public int Index { get; set; }
        public required string Title { get; set; }
        public required string Artist { get; set; }
        public required string Album { get; set; }
        public required string Genre { get; set; }
        public required string CoverUrl { get; set; }
        public required string AudioUrl { get; set; }
        public int LikeCount { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
