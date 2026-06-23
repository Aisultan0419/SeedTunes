FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root

RUN apt-get update && apt-get install -y --no-install-recommends \
    fontconfig \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["SeedTunes/SeedTunes.csproj", "SeedTunes/"]
RUN dotnet restore "SeedTunes/SeedTunes.csproj"
COPY . .
WORKDIR "/src/SeedTunes"
RUN dotnet build "SeedTunes.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SeedTunes.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SeedTunes.dll"]