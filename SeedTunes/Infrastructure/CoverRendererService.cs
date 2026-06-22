using System.Text.Json;
using SkiaSharp;
using Svg.Skia;
using SeedTunes.Contracts;
using SeedTunes.Models;
namespace SeedTunes.Infrastructure
{
    public class CoverRendererService : ICoverRendererService
    {
        private readonly string _assetsPath;

        public CoverRendererService()
        {
            _assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Assets");
        }

        public async Task<string> RenderCoverAsync(string jsonPrompt)
        {
            if (string.IsNullOrWhiteSpace(jsonPrompt))
                return string.Empty;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var promptData = JsonSerializer.Deserialize<CoverPromptData>(jsonPrompt, options);

            if (promptData?.Style == null || promptData.BackgroundShape == null || promptData.HeroAsset == null)
                return string.Empty;

            using var bitmap = new SKBitmap(512, 512);
            using var canvas = new SKCanvas(bitmap);

            canvas.Clear(SKColor.Parse(promptData.Style.Palette.FirstOrDefault() ?? "#000000"));

            long seed = promptData.TrackSeed ?? 0;
            int rotationAngle = (int)(seed % 360);

            DrawAsset(canvas, promptData.BackgroundShape, promptData.Style.Palette, isHero: false, rotationAngle);
            DrawAsset(canvas, promptData.HeroAsset, promptData.Style.Palette, isHero: true, rotationAngle: 0);
            ApplyNoiseEffect(canvas, (float)promptData.Style.Effects.NoiseLevel);

            if (!string.IsNullOrWhiteSpace(promptData.Artist) || !string.IsNullOrWhiteSpace(promptData.Album))
            {
                DrawTextBlock(canvas, promptData.Artist ?? "", promptData.Album ?? "", seed, promptData.Style.Palette);
            }

            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            return $"data:image/png;base64,{Convert.ToBase64String(data.ToArray())}";
        }

        private void DrawTextBlock(SKCanvas canvas, string artist, string album, long seed, List<string> palette)
        {

            string fontPath = Path.Combine(_assetsPath, "BebasNeue-Regular.ttf");
            using var customTypeface = SKTypeface.FromFile(fontPath) ?? SKTypeface.Default;

            using var topGradientPaint = new SKPaint();
            topGradientPaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0), new SKPoint(0, 120),
                new[] { SKColors.Black.WithAlpha(180), SKColors.Transparent },
                null, SKShaderTileMode.Clamp);
            canvas.DrawRect(0, 0, 512, 120, topGradientPaint);

            using var bottomGradientPaint = new SKPaint();
            bottomGradientPaint.Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 392), new SKPoint(0, 512),
                new[] { SKColors.Transparent, SKColors.Black.WithAlpha(200) },
                null, SKShaderTileMode.Clamp);
            canvas.DrawRect(0, 392, 512, 120, bottomGradientPaint);

            bool artistIsShort = artist.Length <= album.Length;
            bool albumIsShort = album.Length < artist.Length;

            float artistFontSize = artistIsShort ? 44f : 36f;
            float albumFontSize = albumIsShort ? 44f : 36f;

            using var artistFont = new SKFont(customTypeface, artistFontSize);
            using var albumFont = new SKFont(customTypeface, albumFontSize);

            using var artistPaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
            using var albumPaint = new SKPaint { Color = SKColors.White.WithAlpha(210), IsAntialias = true };

            artistPaint.ImageFilter = SKImageFilter.CreateDropShadow(0, 2, 6, 6, SKColors.Black.WithAlpha(220));
            albumPaint.ImageFilter = SKImageFilter.CreateDropShadow(0, 2, 4, 4, SKColors.Black.WithAlpha(180));

            artist = TruncateText(artist, artistFont, 480f);
            album = TruncateText(album, albumFont, 480f);

            float artistWidth = artistFont.MeasureText(artist);
            float albumWidth = albumFont.MeasureText(album);

            float artistX = (512f - artistWidth) / 2f;
            float albumX = (512f - albumWidth) / 2f;

            float albumY = 70f; 
            float artistY = 460f;
            canvas.DrawText(artist, artistX, artistY, SKTextAlign.Left, artistFont, artistPaint);
            canvas.DrawText(album, albumX, albumY, SKTextAlign.Left, albumFont, albumPaint);
        }

        private static string TruncateText(string text, SKFont font, float maxWidth)
        {
            if (font.MeasureText(text) <= maxWidth) return text;
            while (text.Length > 1 && font.MeasureText(text + "…") > maxWidth)
                text = text[..^1];
            return text + "…";
        }

        private void DrawAsset(SKCanvas canvas, AssetConfig asset, List<string> palette, bool isHero, float rotationAngle)
        {
            if (asset == null || string.IsNullOrWhiteSpace(asset.AssetId)) return;

            var filePath = Path.Combine(_assetsPath, asset.AssetId);
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"[RENDER ERROR] Файл не найден: {filePath}");
                return;
            }

            using var svg = new SKSvg();
            svg.Load(filePath);
            if (svg.Picture == null) return;

            var bounds = svg.Picture.CullRect;
            var maxDimension = Math.Max(bounds.Width, bounds.Height);
            if (maxDimension <= 0) return;

            float targetSize = (isHero ? 512f : 768f) * (float)asset.Scale;
            float scaleFactor = targetSize / maxDimension;

            canvas.Save();
            var matrix = SKMatrix.CreateTranslation(-bounds.MidX, -bounds.MidY);
            matrix = matrix.PostConcat(SKMatrix.CreateScale(scaleFactor, scaleFactor));
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation(256f, 256f));

            if (!isHero && rotationAngle != 0)
                matrix = matrix.PostConcat(SKMatrix.CreateRotationDegrees(rotationAngle, 256f, 256f));

            canvas.Concat(matrix);

            string hexColor = palette.Count > asset.ColorIndex ? palette[asset.ColorIndex] : "#FFFFFF";
            using var paint = new SKPaint();
            if (isHero)
                paint.ColorFilter = SKColorFilter.CreateBlendMode(SKColor.Parse(hexColor), SKBlendMode.SrcIn);
            else
            {
                paint.ColorFilter = SKColorFilter.CreateBlendMode(SKColor.Parse(hexColor), SKBlendMode.Color);
                paint.Color = paint.Color.WithAlpha(230);
            }

            canvas.DrawPicture(svg.Picture, paint);
            canvas.Restore();
        }

        private static void ApplyNoiseEffect(SKCanvas canvas, float noiseLevel)
        {
            if (noiseLevel <= 0) return;
            using var shader = SKShader.CreatePerlinNoiseFractalNoise(0.1f, 0.1f, 2, 0);
            using var paint = new SKPaint { Shader = shader, BlendMode = SKBlendMode.Overlay, Color = SKColors.White.WithAlpha(80) };
            canvas.Save();
            canvas.DrawRect(new SKRect(0, 0, 512, 512), paint);
            canvas.Restore();
        }
    }
}