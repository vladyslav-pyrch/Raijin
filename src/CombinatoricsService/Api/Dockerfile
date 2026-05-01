FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CombinatoricsService/Api/Raijin.CombinatoricsService.Api.csproj", "src/CombinatoricsService/Api/"]
COPY ["src/ServiceDefaults/Raijin.ServiceDefaults.csproj", "src/ServiceDefaults/"]
COPY ["src/CombinatoricsService/Infrastructure/Raijin.CombinatoricsService.Infrastructure.csproj", "src/CombinatoricsService/Infrastructure/"]
COPY ["src/CombinatoricsService/Application/Raijin.CombinatoricsService.Application.csproj", "src/CombinatoricsService/Application/"]
COPY ["src/CombinatoricsService/Domain/Raijin.CombinatoricsService.Domain.csproj", "src/CombinatoricsService/Domain/"]
RUN dotnet restore "src/CombinatoricsService/Api/Raijin.CombinatoricsService.Api.csproj"
COPY . .
WORKDIR "/src/src/CombinatoricsService/Api"
RUN dotnet build "./Raijin.CombinatoricsService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Raijin.CombinatoricsService.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Raijin.CombinatoricsService.Api.dll"]
