FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./Services/MatchHistoryService/MatchHistoryService.csproj ./Services/MatchHistoryService/
COPY ./SharedApiUtils.Abstractons/SharedApiUtils.Abstractons.csproj ./SharedApiUtils.Abstractons/
COPY ./SharedApiUtils.gRPC/SharedApiUtils.gRPC.csproj ./SharedApiUtils.gRPC/
RUN dotnet restore Services/MatchHistoryService/MatchHistoryService.csproj

COPY ./Services/MatchHistoryService/ ./Services/MatchHistoryService/
COPY ./SharedApiUtils.Abstractons/ ./SharedApiUtils.Abstractons/
COPY ./SharedApiUtils.gRPC/ ./SharedApiUtils.gRPC/
WORKDIR /src/Services/MatchHistoryService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5063
EXPOSE 5062
ENTRYPOINT ["dotnet", "MatchHistoryService.dll"]
