FROM ubuntu:24.04 AS build

RUN apt-get update
RUN apt-get install -y unzip wget
RUN rm -rf /var/lib/apt/lists/*

WORKDIR /app
RUN wget -O cryptominisat.zip https://github.com/msoos/cryptominisat/releases/download/release%2F5.13.0/cryptominisat-linux-arm64.zip
RUN unzip cryptominisat.zip -d cryptominisat
RUN rm cryptominisat.zip
ENV PATH="/app/cryptominisat:$PATH"
RUN mkdir -p /app/cryptominisat/problems
WORKDIR /app/cryptominisat/problems

CMD ["tail", "-f", "/dev/null"]