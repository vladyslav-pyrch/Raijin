FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/CombinatoricsService/SatSolver/Raijin.CombinatoricsService.SatSolver.csproj", "CombinatoricsService/SatSolver/"]
COPY ["src/ServiceDefaults/Raijin.ServiceDefaults.csproj", "ServiceDefaults/"]
COPY ["src/CombinatoricsService/Infrastructure/Raijin.CombinatoricsService.Infrastructure.csproj", "CombinatoricsService/Infrastructure/"]
COPY ["src/CombinatoricsService/Application/Raijin.CombinatoricsService.Application.csproj", "CombinatoricsService/Application/"]
COPY ["src/CombinatoricsService/Domain/Raijin.CombinatoricsService.Domain.csproj", "CombinatoricsService/Domain/"]
RUN dotnet restore "CombinatoricsService/SatSolver/Raijin.CombinatoricsService.SatSolver.csproj"
COPY ["src/CombinatoricsService/SatSolver/.", "CombinatoricsService/SatSolver/"]
COPY ["src/ServiceDefaults/.", "ServiceDefaults/"]
COPY ["src/CombinatoricsService/Infrastructure/.", "CombinatoricsService/Infrastructure/"]
COPY ["src/CombinatoricsService/Application/.", "CombinatoricsService/Application/"]
COPY ["src/CombinatoricsService/Domain/.", "CombinatoricsService/Domain/"]
WORKDIR "/src/CombinatoricsService/SatSolver"
RUN dotnet build "./Raijin.CombinatoricsService.SatSolver.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Raijin.CombinatoricsService.SatSolver.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Raijin.CombinatoricsService.SatSolver.dll"]
