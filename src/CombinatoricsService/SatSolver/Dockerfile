FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM debian:bookworm AS sat_solver-builds

WORKDIR /sat_solvers

RUN apt-get update
RUN apt-get install -y build-essential cmake git libgmp-dev pkg-config
RUN git clone https://github.com/msoos/cryptominisat.git
WORKDIR /sat_solvers/cryptominisat
RUN git checkout release/v5.14.4
RUN mkdir build
WORKDIR /sat_solvers/cryptominisat/build
RUN cmake .. \
    -DCMAKE_BUILD_TYPE=Release \
    -DSTATICCOMPILE=ON \
    -DBUILD_SHARED_LIBS=OFF
RUN make -j$(nproc)
WORKDIR /sat_solvers
RUN git clone https://github.com/arminbiere/cadical.git
WORKDIR /sat_solvers/cadical
RUN git checkout rel-3.0.0
RUN ./configure && make

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

USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID

WORKDIR /app
COPY --from=publish /app/publish .

COPY --from=sat_solver-builds /sat_solvers/cryptominisat/build/ /app/lib/cryptominisat/
COPY --from=sat_solver-builds /sat_solvers/cadical/build/ /app/lib/cadical/

ENV Cryptominisat__ExecutableFilePath=/app/lib/cryptominisat/cryptominisat5
ENV Cadical__ExecutableFilePath=/app/lib/cadical/cadical

ENTRYPOINT ["dotnet", "Raijin.CombinatoricsService.SatSolver.dll"]
