FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./MatchHistoryService.csproj ./MatchHistoryService/
RUN dotnet restore MatchHistoryService/MatchHistoryService.csproj

COPY . ./MatchHistoryService/
WORKDIR /src/MatchHistoryService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5063
ENTRYPOINT ["dotnet", "MatchHistoryService.dll"]
