
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./Services/GameSessionService/GameSessionService.csproj ./Services/GameSessionService/
COPY ./SharedApiUtils/SharedApiUtils.csproj ./SharedApiUtils/
RUN dotnet restore Services/GameSessionService/GameSessionService.csproj

COPY ./Services/GameSessionService/ ./Services/GameSessionService/
COPY ./SharedApiUtils/ ./SharedApiUtils/
WORKDIR /src/Services/GameSessionService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5087
ENTRYPOINT ["dotnet", "GameSessionService.dll"]