
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./RatingService.csproj ./RatingService/
RUN dotnet restore RatingService/RatingService.csproj

COPY . ./RatingService/
WORKDIR /src/RatingService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5263
ENTRYPOINT ["dotnet", "RatingService.dll"]