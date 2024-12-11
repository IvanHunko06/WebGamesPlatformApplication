FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./Services/GamesService/GamesService.csproj ./Services/GamesService/
COPY ./SharedApiUtils/SharedApiUtils.csproj ./SharedApiUtils/
RUN dotnet restore Services/GamesService/GamesService.csproj

COPY ./Services/GamesService/ ./Services/GamesService/
COPY ./SharedApiUtils/ ./SharedApiUtils/
WORKDIR /src/Services/GamesService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5247
ENTRYPOINT ["dotnet", "GamesService.dll"]
