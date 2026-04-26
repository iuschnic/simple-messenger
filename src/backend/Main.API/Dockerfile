FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Main.API/Main.API.csproj", "Main.API/"]
COPY ["Main.Realtime.BR/Main.Realtime.BR.csproj", "Main.Realtime.BR/"]
COPY ["Shared.Main.Auth/Shared.Main.Auth.csproj", "Shared.Main.Auth/"]
COPY ["Main.BL/Main.BL.csproj", "Main.BL/"]
COPY ["Shared.Main.Realtime/Shared.Main.Realtime.csproj", "Shared.Main.Realtime/"]
COPY ["Main.Application/Main.Application.csproj", "Main.Application/"]
COPY ["Main.DB/Main.DB.csproj", "Main.DB/"]
COPY ["Main.Auth.BR/Main.Auth.BR.csproj", "Main.Auth.BR/"]
RUN dotnet restore "Main.API/Main.API.csproj"
COPY . .
WORKDIR "/src/Main.API"
RUN dotnet build "./Main.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Main.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Main.API.dll", "--urls", "http://*:8080"]
