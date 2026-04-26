FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Auth.API/Auth.API.csproj", "Auth.API/"]
COPY ["Shared.Main.Auth/Shared.Main.Auth.csproj", "Shared.Main.Auth/"]
COPY ["Auth.BL/Auth.BL.csproj", "Auth.BL/"]
COPY ["Auth.Main.BR/Auth.Main.BR.csproj", "Auth.Main.BR/"]
COPY ["Auth.DB/Auth.DB.csproj", "Auth.DB/"]
RUN dotnet restore "Auth.API/Auth.API.csproj"
COPY . .
WORKDIR "/src/Auth.API"
RUN dotnet build "./Auth.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Auth.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auth.API.dll", "--urls", "http://*:8080"]
