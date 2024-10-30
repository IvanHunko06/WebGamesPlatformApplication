#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./RoomsService.csproj ./RoomsService/
RUN dotnet restore RoomsService/RoomsService.csproj

COPY . ./RoomsService/
WORKDIR /src/RoomsService
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5063
ENTRYPOINT ["dotnet", "RoomsService.dll"]