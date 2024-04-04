#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/MpSo.API/MpSo.API.csproj", "src/MpSo.API/"]
COPY ["src/MpSo/MpSo.csproj", "src/MpSo/"]
COPY ["tests/MpSo.UnitTests/MpSo.UnitTests.csproj", "tests/MpSo.UnitTests/"]
COPY ["tests/MpSo.IntegrationTests/MpSo.IntegrationTests.csproj", "tests/MpSo.IntegrationTests/"]
RUN dotnet restore "./src/MpSo.API/MpSo.API.csproj"
RUN dotnet restore "./src/MpSo/MpSo.csproj"
RUN dotnet restore "./tests/MpSo.UnitTests/MpSo.UnitTests.csproj"
RUN dotnet restore "./tests/MpSo.IntegrationTests/MpSo.IntegrationTests.csproj"
COPY . .
WORKDIR "/src/src/MpSo.API"
RUN dotnet build "./MpSo.API.csproj" -c $BUILD_CONFIGURATION -o /app/build
RUN dotnet test "../../tests/MpSo.UnitTests/MpSo.UnitTests.csproj" -c $BUILD_CONFIGURATION

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./MpSo.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MpSo.API.dll"]