
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./Services/RatingService/RatingService.csproj ./Services/RatingService/
COPY ./SharedApiUtils/SharedApiUtils.csproj ./SharedApiUtils/
RUN dotnet restore Services/RatingService/RatingService.csproj

COPY ./Services/RatingService/ ./Services/RatingService/
COPY ./SharedApiUtils/ ./SharedApiUtils/
WORKDIR /src/Services/RatingService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5263
ENTRYPOINT ["dotnet", "RatingService.dll"]