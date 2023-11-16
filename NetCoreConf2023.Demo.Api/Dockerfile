#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NetCoreConf2023.Demo.Api/NetCoreConf2023.Demo.Api.csproj", "NetCoreConf2023.Demo.Api/"]
RUN dotnet restore "./NetCoreConf2023.Demo.Api/./NetCoreConf2023.Demo.Api.csproj"
COPY . .
WORKDIR "/src/NetCoreConf2023.Demo.Api"
RUN dotnet build "./NetCoreConf2023.Demo.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NetCoreConf2023.Demo.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NetCoreConf2023.Demo.Api.dll"]
