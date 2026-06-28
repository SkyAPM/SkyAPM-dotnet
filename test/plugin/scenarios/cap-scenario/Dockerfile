# CAP plugin-test scenario image. Build context MUST be the repo root so the agent
# source + the protocol-v3 submodule are available. TFM + CAP_VERSION select the
# matrix entry under test.
#   docker build -f test/plugin/scenarios/cap-scenario/Dockerfile \
#     --build-arg DOTNET_VERSION=8.0 --build-arg TFM=net8.0 --build-arg CAP_VERSION=8.0.0 -t cap-scenario .
ARG DOTNET_VERSION=8.0

# Build with the .NET 10 SDK (targets both net8.0 and net10.0).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TFM=net8.0
ARG CAP_VERSION=8.0.0
WORKDIR /src
COPY . .
# Generate the gRPC/protobuf code from the protocol-v3 submodule before the transports compile.
RUN dotnet build src/SkyApm.Transport.Protocol -c Release
RUN dotnet publish test/plugin/scenarios/cap-scenario/Case.csproj -c Release \
        -f ${TFM} -p:TFM=${TFM} -p:CapVersion=${CAP_VERSION} -o /app

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS run
WORKDIR /app
COPY --from=build /app .
# Activate the agent zero-code (note casing: SkyAPM). The collector address is in skyapm.json.
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore \
    ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Case.dll"]
