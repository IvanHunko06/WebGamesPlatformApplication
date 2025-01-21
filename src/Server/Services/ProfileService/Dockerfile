
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./Services/ProfileService/ProfileService.csproj ./Services/ProfileService/
COPY ./SharedApiUtils.Abstractons/SharedApiUtils.Abstractons.csproj ./SharedApiUtils.Abstractons/
COPY ./SharedApiUtils.RabbitMq/SharedApiUtils.RabbitMq.csproj ./SharedApiUtils.RabbitMq/
RUN dotnet restore Services/ProfileService/ProfileService.csproj

COPY ./Services/ProfileService/ ./Services/ProfileService/
COPY ./SharedApiUtils.Abstractons/ ./SharedApiUtils.Abstractons/
COPY ./SharedApiUtils.RabbitMq/ ./SharedApiUtils.RabbitMq/
WORKDIR /src/Services/ProfileService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5151
ENTRYPOINT ["dotnet", "ProfileService.dll"]