FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM base AS cryptominisat
USER root
WORKDIR /app/cryptominisat
RUN apt-get update \
    && apt-get install -y unzip wget \
    && rm -rf /var/lib/apt/lists/*
RUN wget -O cryptominisat.zip https://github.com/msoos/cryptominisat/releases/download/release%2F5.13.0/cryptominisat-linux-arm64.zip \
    && unzip cryptominisat.zip \
    && rm cryptominisat.zip

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["./Worker/Raijin.SatSolver.Worker.csproj", "./SatSolver/Worker/"]
COPY ["./Infrastructure/Raijin.SatSolver.Infrastructure.csproj", "./SatSolver/Infrastructure/"]
COPY ["./Application/Raijin.SatSolver.Application.csproj", "./SatSolver/Application/"]
COPY ["./Domain/Raijin.SatSolver.Domain.csproj", "./SatSolver/Domain/"]
RUN dotnet restore "./SatSolver/Worker/Raijin.SatSolver.Worker.csproj"
COPY [".", "./SatSolver/"]
WORKDIR "/src/SatSolver/Worker"
RUN dotnet build "./Raijin.SatSolver.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Raijin.SatSolver.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=cryptominisat /app/cryptominisat/ /usr/bin
ENV PATH="/usr/bin:$PATH"

ENTRYPOINT ["dotnet", "Raijin.SatSolver.Worker.dll"]
