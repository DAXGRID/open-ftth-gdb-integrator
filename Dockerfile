FROM mcr.microsoft.com/dotnet/core/sdk:3.1.402-alpine3.12 AS build-env
WORKDIR /app

COPY . ./

WORKDIR /app/src/OpenFTTH.GDBIntegrator
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1.8-alpine3.12
WORKDIR /app

COPY --from=build-env /app/src/OpenFTTH.GDBIntegrator/out .
ENTRYPOINT ["dotnet", "OpenFTTH.GDBIntegrator.dll"]
