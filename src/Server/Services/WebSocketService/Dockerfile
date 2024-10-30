
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./WebSocketService.csproj ./WebSocketService/
RUN dotnet restore WebSocketService/WebSocketService.csproj

COPY . ./WebSocketService/
WORKDIR /src/WebSocketService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5059
ENTRYPOINT ["dotnet", "WebSocketService.dll"]