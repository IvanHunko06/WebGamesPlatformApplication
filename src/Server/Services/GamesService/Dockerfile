FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./GamesService.csproj ./GamesService/
RUN dotnet restore GamesService/GamesService.csproj

COPY . ./GamesService/
WORKDIR /src/GamesService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5247
ENTRYPOINT ["dotnet", "GamesService.dll"]
