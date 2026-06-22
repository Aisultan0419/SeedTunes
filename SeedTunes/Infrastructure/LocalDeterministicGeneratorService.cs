using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SeedTunes.Models;
using SeedTunes.Contracts;

namespace SeedTunes.Infrastructure
{
    public class LocalDeterministicGeneratorService : IAiMusicGeneratorService
    {
        private readonly Dictionary<string, GenreRules> _genreMatrix;

        public LocalDeterministicGeneratorService()
        {
            var matrixPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "MusicGenreMatrix.json");
            if (!File.Exists(matrixPath))
            {
                throw new FileNotFoundException($"[MATRIX ERROR] Файл конфигурации не найден по пути: {matrixPath}");
            }

            var jsonText = File.ReadAllText(matrixPath);
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            _genreMatrix = JsonSerializer.Deserialize<Dictionary<string, GenreRules>>(jsonText, jsonSerializerOptions)
                           ?? new Dictionary<string, GenreRules>();
        }

        public async Task EnrichWithAiAsync(List<AiMusicMetadata> batch, string languageCode)
        {
            foreach (var item in batch)
            {
                var genreKey = NormalizeGenre(item.Genre);
                if (!_genreMatrix.TryGetValue(genreKey, out var rules))
                {
                    rules = _genreMatrix.GetValueOrDefault("Lo-Fi") ?? _genreMatrix.Values.First();
                }

                int compressedSeed = (int)(item.TrackSeed ^ (item.TrackSeed >> 32));
                var rng = new Random(compressedSeed);

                var selectedPalette = rules.Palettes[rng.Next(rules.Palettes.Count)];
                var selectedBackground = rules.BackgroundShapes[rng.Next(rules.BackgroundShapes.Count)];
                var selectedHero = rules.HeroAssets[rng.Next(rules.HeroAssets.Count)];

                var glass = Lerp(rules.Effects.GlassIntensity[0], rules.Effects.GlassIntensity[1], rng.NextDouble());
                var noise = Lerp(rules.Effects.NoiseLevel[0], rules.Effects.NoiseLevel[1], rng.NextDouble());
                var glitch = Lerp(rules.Effects.GlitchIntensity[0], rules.Effects.GlitchIntensity[1], rng.NextDouble());

                var styleObj = new
                {
                    palette = selectedPalette,
                    mood = genreKey.ToLower(),
                    effects = new
                    {
                        glassIntensity = Math.Round(glass, 2),
                        noiseLevel = Math.Round(noise, 2),
                        glitchIntensity = Math.Round(glitch, 2)
                    }
                };

                var backgroundShapeObj = new
                {
                    assetId = MapAssetIdToFileName(selectedBackground),
                    colorIndex = 2, 
                    scale = 0.85 + (rng.NextDouble() * 0.15) 
                };

                var heroAssetObj = new
                {
                    assetId = MapAssetIdToFileName(selectedHero),
                    colorIndex = 1, 
                    scale = 0.40 + (rng.NextDouble() * 0.15) 
                };

                item.CoverPrompt = JsonSerializer.Serialize(new
                {
                    style = styleObj,
                    backgroundShape = backgroundShapeObj,
                    heroAsset = heroAssetObj,
                    artist = item.Artist,
                    album = item.Album
                }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }

            await Task.CompletedTask;
        }

        private static string NormalizeGenre(string genre)
        {
            if (string.IsNullOrWhiteSpace(genre)) return "Lo-Fi";
            var cleaned = genre.Trim().ToLower();
            if (cleaned.Contains("trap") || cleaned.Contains("hip-hop")) return "Trap";
            if (cleaned.Contains("lo-fi") || cleaned.Contains("lofi")) return "Lo-Fi";
            if (cleaned.Contains("edm") || cleaned.Contains("house")) return "House";
            if (cleaned.Contains("synthwave")) return "Synthwave";
            if (cleaned.Contains("rock") || cleaned.Contains("metal")) return "Metal";
            if (cleaned.Contains("disco") || cleaned.Contains("pop")) return "Pop";

            return "Lo-Fi";
        }

        private static string MapAssetIdToFileName(string assetId)
        {
            var cleanId = assetId.Replace(".svg", "").ToLower();

            return cleanId switch
            {
                "update_arrow" => "update-arrow-svgrepo-com.svg",
                "compass_outline" => "compass-outline-svgrepo-com.svg",
                "heart_pulse" => "heart-pulse-svgrepo-com.svg",
                "qr_code" => "qr-code-svgrepo-com.svg",
                "scan_code" => "scan-code-svgrepo-com.svg",
                "bar_code" => "bar-code-svgrepo-com.svg",
                "rose_flower" => "rose-flower-svgrepo-com.svg",
                "synth_keys" => "synthesizer-keyboard-svgrepo-com.svg",
                "drums_rhythm" => "drums-rhythm-loud-play-svgrepo-com.svg",
                "drum_kit" => "drum-kit-drums-rock-set-svgrepo-com.svg",
                "bass_guitar" => "guitar-instrument-bass-electric-svgrepo-com.svg",
                "electric_guitar_1" => "electric-guitar-with-sharp-tip-edges-for-rockstar-svgrepo-com.svg",
                "electric_guitar_2" => "electric-guitar-2-svgrepo-com.svg",
                "crown" => "crown-minimalistic-svgrepo-com.svg",
                "vinyl" => "vinyl-record-svgrepo-com.svg",
                "speaker_low" => "low-volume-speaker-svgrepo-com.svg",
                "audio_speaker" => "audio-speaker-svgrepo-com.svg",
                "comedy_mask" => "mask-comedy-svgrepo-com.svg",
                "disco_ball" => "disco-ball-disco-svgrepo-com.svg",
                "soda_cup" => "soda-cup-svgrepo-com.svg",
                "theater_masks" => "theater-masks-svgrepo-com.svg",
                "video_player" => "video-player-sign-svgrepo-com.svg",
                "shoes" => "women-pair-of-shoes-svgrepo-com.svg",
                "3d_glasses" => "cinema-3d-vintage-glasses-svgrepo-com.svg",
                "retro_icons" => "retro-icons-04-svgrepo-com.svg",
                "cassette" => "cassette-retro-svgrepo-com.svg",


                _ => assetId.EndsWith(".svg") ? assetId : $"{assetId}.svg"
            };
        }

        private static double Lerp(double min, double max, double value)
        {
            return min + (max - min) * value;
        }
    }
}