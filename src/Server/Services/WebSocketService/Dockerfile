
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./Services/WebSocketService/WebSocketService.csproj ./Services/WebSocketService/
COPY ./SharedApiUtils.Abstractons/SharedApiUtils.Abstractons.csproj ./SharedApiUtils.Abstractons/
COPY ./SharedApiUtils.gRPC/SharedApiUtils.gRPC.csproj ./SharedApiUtils.gRPC/
COPY ./SharedApiUtils.RabbitMq/SharedApiUtils.RabbitMq.csproj ./SharedApiUtils.RabbitMq/
RUN dotnet restore Services/WebSocketService/WebSocketService.csproj

COPY ./Services/WebSocketService/ ./Services/WebSocketService/
COPY ./SharedApiUtils.Abstractons/ ./SharedApiUtils.Abstractons/
COPY ./SharedApiUtils.gRPC/ ./SharedApiUtils.gRPC/
COPY ./SharedApiUtils.RabbitMq/ ./SharedApiUtils.RabbitMq/
WORKDIR /src/Services/WebSocketService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5059
EXPOSE 5060
ENTRYPOINT ["dotnet", "WebSocketService.dll"]