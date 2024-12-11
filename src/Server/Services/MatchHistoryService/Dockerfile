FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./Services/MatchHistoryService/MatchHistoryService.csproj ./Services/MatchHistoryService/
COPY ./SharedApiUtils/SharedApiUtils.csproj ./SharedApiUtils/
RUN dotnet restore Services/MatchHistoryService/MatchHistoryService.csproj

COPY ./Services/MatchHistoryService/ ./Services/MatchHistoryService/
COPY ./SharedApiUtils/ ./SharedApiUtils/
WORKDIR /src/Services/MatchHistoryService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5063
ENTRYPOINT ["dotnet", "MatchHistoryService.dll"]
