
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ./Services/GameProcessing/TicTacToeGameProcessing/TicTacToeGameProcessing.csproj ./Services/GameProcessing/TicTacToeGameProcessing/
COPY ./SharedApiUtils/SharedApiUtils.csproj ./SharedApiUtils/
RUN dotnet restore Services/GameProcessing/TicTacToeGameProcessing/TicTacToeGameProcessing.csproj

COPY ./Services/GameProcessing/TicTacToeGameProcessing/ ./Services/GameProcessing/TicTacToeGameProcessing/
COPY ./SharedApiUtils/ ./SharedApiUtils/
WORKDIR /src/Services/GameProcessing/TicTacToeGameProcessing
RUN dotnet publish -c Release -o /app/publish


FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 5203
ENTRYPOINT ["dotnet", "TicTacToeGameProcessing.dll"]