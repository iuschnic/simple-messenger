FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Realtime.API/Realtime.API.csproj", "Realtime.API/"]
COPY ["Shared.Main.Realtime/Shared.Main.Realtime.csproj", "Shared.Main.Realtime/"]
COPY ["Realtime.Main.BR/Realtime.Main.BR.csproj", "Realtime.Main.BR/"]
COPY ["Realtime.BL/Realtime.BL.csproj", "Realtime.BL/"]
COPY ["Realtime.DB/Realtime.DB.csproj", "Realtime.DB/"]
RUN dotnet restore "Realtime.API/Realtime.API.csproj"
COPY . .
WORKDIR "/src/Realtime.API"
RUN dotnet build "./Realtime.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Realtime.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Realtime.API.dll", "--urls", "http://*:8080"]
