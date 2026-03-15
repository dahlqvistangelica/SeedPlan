# 1. Build-stadiet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiera projektfiler för att cacha lager (viktigt för snabba byggen)
COPY ["SeedPlan/SeedPlan.csproj", "SeedPlan/"]
COPY ["SeedPlan.Client/SeedPlan.Client.csproj", "SeedPlan.Client/"]
COPY ["Shared/Shared.csproj", "Shared/"]

# Restorea beroenden
RUN dotnet restore "SeedPlan/SeedPlan.csproj"

# Kopiera resten av koden och bygg
COPY . .
RUN dotnet publish "SeedPlan/SeedPlan.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 2. Runtime-stadiet
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

# Railway använder port 8080 som standard
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SeedPlan.dll"]