# Context: backend/ (compose: context ../backend). Targets: dev (dotnet watch), build, prod.
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS dev

RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"

WORKDIR /app

# Without nuget.config the restore sees nuget.org only and fails NU1101 for Innovayse.Auth.
COPY nuget.config ./nuget.config

COPY Innovayse.Hostpanel.sln ./Innovayse.Hostpanel.sln
COPY Directory.Build.props ./Directory.Build.props
COPY Directory.Packages.props ./Directory.Packages.props
COPY src/Directory.Build.props ./src/Directory.Build.props
COPY src/SharedKernel/Innovayse.Hostpanel.SharedKernel.csproj ./src/SharedKernel/
COPY src/Shared/Innovayse.Hostpanel.Shared.csproj ./src/Shared/
COPY src/Innovayse.Hostpanel.Domain/Innovayse.Hostpanel.Domain.csproj ./src/Innovayse.Hostpanel.Domain/
COPY src/Innovayse.Hostpanel.Application/Innovayse.Hostpanel.Application.csproj ./src/Innovayse.Hostpanel.Application/
COPY src/Innovayse.Hostpanel.Infrastructure/Innovayse.Hostpanel.Infrastructure.csproj ./src/Innovayse.Hostpanel.Infrastructure/
COPY src/Innovayse.Hostpanel.API/Innovayse.Hostpanel.API.csproj ./src/Innovayse.Hostpanel.API/
COPY src/Innovayse.SDK/Innovayse.SDK.csproj ./src/Innovayse.SDK/
COPY src/Innovayse.Providers.CWP/Innovayse.Providers.CWP.csproj ./src/Innovayse.Providers.CWP/
COPY src/Innovayse.Providers.CWP7/Innovayse.Providers.CWP7.csproj ./src/Innovayse.Providers.CWP7/
COPY src/Innovayse.Providers.Inecobank/Innovayse.Providers.Inecobank.csproj ./src/Innovayse.Providers.Inecobank/
COPY tests/Innovayse.Hostpanel.Domain.Tests/Innovayse.Hostpanel.Domain.Tests.csproj ./tests/Innovayse.Hostpanel.Domain.Tests/
COPY tests/Innovayse.Hostpanel.Architecture.Tests/Innovayse.Hostpanel.Architecture.Tests.csproj ./tests/Innovayse.Hostpanel.Architecture.Tests/
COPY tests/Innovayse.Hostpanel.Integration.Tests/Innovayse.Hostpanel.Integration.Tests.csproj ./tests/Innovayse.Hostpanel.Integration.Tests/
COPY tests/Innovayse.CWP.Tests/Innovayse.CWP.Tests.csproj ./tests/Innovayse.CWP.Tests/
COPY tests/Innovayse.Inecobank.Tests/Innovayse.Inecobank.Tests.csproj ./tests/Innovayse.Inecobank.Tests/
# NuGet reads this exact name from the environment; it must be a declared ARG to reach the restore.
ARG NuGetPackageSourceCredentials_innovayse
# AUTH_MODE=sso builds with -p:InnovayseSso=true so a missing registry credential fails here, not at boot.
ARG AUTH_MODE

RUN SSO=$([ "$AUTH_MODE" = "sso" ] && echo "-p:InnovayseSso=true"); \
    dotnet restore Innovayse.Hostpanel.sln $SSO

# Documentation only; the listening port is ASPNETCORE_HTTP_PORTS (API_PORT) from compose.
EXPOSE 5148

ENTRYPOINT ["dotnet", "watch", "run", \
    "--project", "src/Innovayse.Hostpanel.API/Innovayse.Hostpanel.API.csproj", \
    "--no-launch-profile", \
    "--"]

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

COPY nuget.config ./nuget.config

COPY Innovayse.Hostpanel.sln ./Innovayse.Hostpanel.sln
COPY Directory.Build.props ./Directory.Build.props
COPY Directory.Packages.props ./Directory.Packages.props
COPY src/Directory.Build.props ./src/Directory.Build.props
COPY src/SharedKernel/Innovayse.Hostpanel.SharedKernel.csproj ./src/SharedKernel/
COPY src/Shared/Innovayse.Hostpanel.Shared.csproj ./src/Shared/
COPY src/Innovayse.Hostpanel.Domain/Innovayse.Hostpanel.Domain.csproj ./src/Innovayse.Hostpanel.Domain/
COPY src/Innovayse.Hostpanel.Application/Innovayse.Hostpanel.Application.csproj ./src/Innovayse.Hostpanel.Application/
COPY src/Innovayse.Hostpanel.Infrastructure/Innovayse.Hostpanel.Infrastructure.csproj ./src/Innovayse.Hostpanel.Infrastructure/
COPY src/Innovayse.Hostpanel.API/Innovayse.Hostpanel.API.csproj ./src/Innovayse.Hostpanel.API/
COPY src/Innovayse.SDK/Innovayse.SDK.csproj ./src/Innovayse.SDK/
COPY src/Innovayse.Providers.CWP/Innovayse.Providers.CWP.csproj ./src/Innovayse.Providers.CWP/
COPY src/Innovayse.Providers.CWP7/Innovayse.Providers.CWP7.csproj ./src/Innovayse.Providers.CWP7/
COPY src/Innovayse.Providers.Inecobank/Innovayse.Providers.Inecobank.csproj ./src/Innovayse.Providers.Inecobank/
COPY tests/Innovayse.Hostpanel.Domain.Tests/Innovayse.Hostpanel.Domain.Tests.csproj ./tests/Innovayse.Hostpanel.Domain.Tests/
COPY tests/Innovayse.Hostpanel.Architecture.Tests/Innovayse.Hostpanel.Architecture.Tests.csproj ./tests/Innovayse.Hostpanel.Architecture.Tests/
COPY tests/Innovayse.Hostpanel.Integration.Tests/Innovayse.Hostpanel.Integration.Tests.csproj ./tests/Innovayse.Hostpanel.Integration.Tests/
COPY tests/Innovayse.CWP.Tests/Innovayse.CWP.Tests.csproj ./tests/Innovayse.CWP.Tests/
COPY tests/Innovayse.Inecobank.Tests/Innovayse.Inecobank.Tests.csproj ./tests/Innovayse.Inecobank.Tests/
ARG NuGetPackageSourceCredentials_innovayse
ARG AUTH_MODE

RUN SSO=$([ "$AUTH_MODE" = "sso" ] && echo "-p:InnovayseSso=true"); \
    dotnet restore Innovayse.Hostpanel.sln $SSO

COPY . .
RUN SSO=$([ "$AUTH_MODE" = "sso" ] && echo "-p:InnovayseSso=true"); \
    dotnet publish src/Innovayse.Hostpanel.API/Innovayse.Hostpanel.API.csproj -c Release -o /out --no-restore /p:ErrorOnDuplicatePublishOutputFiles=false $SSO

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS prod

# icu-data-full: the Alpine image is culture-invariant by default, and the API starts with en, ru and hy.
RUN apk add --no-cache icu-libs icu-data-full
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app
COPY --from=build /out .

# Documentation only; the listening port is ASPNETCORE_HTTP_PORTS (API_PORT) from compose.
EXPOSE 5148

ENTRYPOINT ["dotnet", "Innovayse.Hostpanel.API.dll"]
