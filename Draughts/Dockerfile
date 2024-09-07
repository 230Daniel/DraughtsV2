FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY Draughts.Api/Draughts.Api.csproj Draughts.Api/
COPY Draughts.GameLogic/Draughts.GameLogic.csproj Draughts.GameLogic/
RUN dotnet restore Draughts.Api/Draughts.Api.csproj
COPY . .
WORKDIR /src/Draughts.Api
RUN dotnet build Draughts.Api.csproj -c Release -o /app/build

FROM build AS publish
RUN dotnet publish Draughts.Api.csproj -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Draughts.Api.dll"]
