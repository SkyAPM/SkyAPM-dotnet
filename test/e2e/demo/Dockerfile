# Multi-stage build for the e2e demo. Build context MUST be the repo root so the
# agent source + the protocol-v3 submodule are available:
#   docker build -f test/e2e/demo/Dockerfile --build-arg DOTNET_VERSION=8.0 --build-arg TFM=net8.0 -t skyapm-e2e-demo:net8 .
ARG DOTNET_VERSION=8.0

# Always build with the .NET 10 SDK — it can target both net8.0 and net10.0
# (the agent's shared libs multi-target net8.0;net10.0;netstandard2.0).
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG TFM=net8.0
WORKDIR /src
COPY . .
RUN dotnet publish test/e2e/demo/Demo.csproj -c Release -f ${TFM} -o /app

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS run
WORKDIR /app
COPY --from=build /app .
# Activate the agent (note casing: SkyAPM). Service name + OAP address come from env.
ENV ASPNETCORE_HOSTINGSTARTUPASSEMBLIES=SkyAPM.Agent.AspNetCore \
    ASPNETCORE_URLS=http://+:8080 \
    DEMO_SELF_URL=http://localhost:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Demo.dll"]
