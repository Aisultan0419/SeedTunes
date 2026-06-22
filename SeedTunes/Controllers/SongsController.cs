using Microsoft.AspNetCore.Mvc;
using SeedTunes.Contracts;

namespace SeedTunes.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly ISongService _songService;

        public SongsController(ISongService songService)
        {
            _songService = songService;
        }

        [HttpGet("page")]
        public async Task<IActionResult> GetPage(
            [FromQuery] ulong userSeed,
            [FromQuery] int pageNumber,
            [FromQuery] double averageLikes,
            [FromQuery] string languageCode = "en")
        {
            var pageData = await _songService.GetSongsPageAsync(userSeed, pageNumber, languageCode, averageLikes);
            return Ok(pageData);
        }

        [HttpGet("audio")]
        public IActionResult GetAudio(
            [FromQuery] ulong userSeed,
            [FromQuery] int pageNumber,
            [FromQuery] int songIndex)
        {
            byte[] midiBytes = _songService.GenerateAudio(userSeed, pageNumber, songIndex);
            return File(midiBytes, "audio/midi", $"track_{pageNumber}_{songIndex}.mid");
        }
    }
}
