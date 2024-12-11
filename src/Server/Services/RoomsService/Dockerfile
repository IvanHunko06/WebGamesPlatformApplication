FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./Services/RoomsService/RoomsService.csproj ./Services/RoomsService/
COPY ./SharedApiUtils/SharedApiUtils.csproj ./SharedApiUtils/
RUN dotnet restore Services/RoomsService/RoomsService.csproj

COPY ./Services/RoomsService/ ./Services/RoomsService/
COPY ./SharedApiUtils/ ./SharedApiUtils/
WORKDIR /src/Services/RoomsService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5063
ENTRYPOINT ["dotnet", "RoomsService.dll"]