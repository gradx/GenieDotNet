ARG REPO=mcr.microsoft.com/dotnet/aspnet
FROM $REPO:8.0.1-jammy-amd64 AS build-env


ENV \
    # Do not generate certificate
    DOTNET_GENERATE_ASPNET_CERTIFICATE=false \
    # Do not show first run text
    DOTNET_NOLOGO=true \
    # SDK version
    DOTNET_SDK_VERSION=8.0.101 \
    # Enable correct mode for dotnet watch (only mode supported in a container)
    DOTNET_USE_POLLING_FILE_WATCHER=true \
    # Skip extraction of XML docs - generally not useful within an image/container - helps performance
    NUGET_XMLDOC_MODE=skip \
    # PowerShell telemetry for docker image usage
    POWERSHELL_DISTRIBUTION_CHANNEL=PSDocker-DotnetSDK-Ubuntu-22.04

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        cron \
        rsyslog \
        iptables \
        procps \
        curl \
        bash \
        dos2unix \
        build-essential \
        cmake \
        automake \
        libtool \
        autoconf \
        kmod \
        git \
        wget

# Install .NET SDK
RUN curl -fSL --output dotnet.tar.gz https://dotnetcli.azureedge.net/dotnet/Sdk/$DOTNET_SDK_VERSION/dotnet-sdk-$DOTNET_SDK_VERSION-linux-x64.tar.gz \
    && dotnet_sha512='26df0151a3a59c4403b52ba0f0df61eaa904110d897be604f19dcaa27d50860c82296733329cb4a3cf20a2c2e518e8f5d5f36dfb7931bf714a45e46b11487c9a' \
    && echo "$dotnet_sha512  dotnet.tar.gz" | sha512sum -c - \
    && mkdir -p /usr/share/dotnet \
    && tar -oxzf dotnet.tar.gz -C /usr/share/dotnet ./packs ./sdk ./sdk-manifests ./templates ./LICENSE.txt ./ThirdPartyNotices.txt \
    && rm dotnet.tar.gz \
    # Trigger first run experience by running arbitrary cmd
    && dotnet help

# Copy csproj and restore as distinct layers
WORKDIR /app
COPY . .

WORKDIR /app/Genie.Web.Api
RUN dotnet publish Genie.Web.Api.csproj -c Release -o /app/api_out

WORKDIR /app/api_out
#RUN chmod a+x start.sh

######################################
# Use a previous stage as a new stage
#####################################
FROM build-env AS deploy-env

#Install Azure Cli
RUN curl -sL https://aka.ms/InstallAzureCLIDeb | bash

# Setup http/3
RUN wget https://packages.microsoft.com/config/debian/12/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
RUN dpkg -i packages-microsoft-prod.deb
RUN apt-get update
RUN apt-get install -y --no-install-recommends libmsquic

# copy build files
COPY --from=build-env /app/api_out /api
COPY . /source

RUN rm -rf /app

WORKDIR /api

ENTRYPOINT ["dotnet", "Genie.Web.Api.dll"]
