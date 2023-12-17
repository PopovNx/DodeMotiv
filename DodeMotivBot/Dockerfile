FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine  AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DodeMotivBot/DodeMotivBot.csproj", "DodeMotivBot/"]
RUN dotnet restore "DodeMotivBot/DodeMotivBot.csproj"
COPY . .
WORKDIR "/src/DodeMotivBot"
RUN dotnet build "DodeMotivBot.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DodeMotivBot.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DodeMotivBot.dll"]
