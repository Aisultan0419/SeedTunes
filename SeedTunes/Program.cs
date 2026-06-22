using SeedTunes.Contracts;
using SeedTunes.Infrastructure;
using SeedTunes.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<IAiMusicGeneratorService, LocalDeterministicGeneratorService>();
builder.Services.AddScoped<IMidiGeneratorService, MidiGeneratorService>();
builder.Services.AddSingleton<ITrackSeedGenerator, TrackSeedGenerator>();
builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<ICoverRendererService, CoverRendererService>();
builder.Services.AddCors(options => options.AddPolicy("AllowAll",
    p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));
var app = builder.Build();

app.UseCors("AllowAll");
app.UseHttpsRedirection();

app.UseAuthorization(); 

app.MapControllers();

app.Run();
