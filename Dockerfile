FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY ./*sln ./

COPY ./src/OpenFTTH.GDBIntegrator/*.csproj ./src/OpenFTTH.GDBIntegrator/
COPY ./src/OpenFTTH.GDBIntegrator.Config/*.csproj ./src/OpenFTTH.GDBIntegrator.Config/
COPY ./src/OpenFTTH.GDBIntegrator.RouteNetwork/*.csproj ./src/OpenFTTH.GDBIntegrator.RouteNetwork/
COPY ./src/OpenFTTH.GDBIntegrator.Subscriber/*.csproj ./src/OpenFTTH.GDBIntegrator.Subscriber/
COPY ./src/OpenFTTH.GDBIntegrator.Producer/*.csproj ./src/OpenFTTH.GDBIntegrator.Producer/
COPY ./src/OpenFTTH.GDBIntegrator.GeoDatabase/*.csproj ./src/OpenFTTH.GDBIntegrator.GeoDatabase/
COPY ./src/OpenFTTH.GDBIntegrator.Integrator/*.csproj ./src/OpenFTTH.GDBIntegrator.Integrator/

COPY ./test/OpenFTTH.GDBIntegrator.Tests/*.csproj ./test/OpenFTTH.GDBIntegrator.Tests/
COPY ./test/OpenFTTH.GDBIntegrator.Config.Tests/*.csproj ./test/OpenFTTH.GDBIntegrator.Config.Tests/
COPY ./test/OpenFTTH.GDBIntegrator.RouteNetwork.Tests/*.csproj ./test/OpenFTTH.GDBIntegrator.RouteNetwork.Tests/
COPY ./test/OpenFTTH.GDBIntegrator.Subscriber.Tests/*.csproj ./test/OpenFTTH.GDBIntegrator.Subscriber.Tests/
COPY ./test/OpenFTTH.GDBIntegrator.Producer.Tests/*.csproj ./test/OpenFTTH.GDBIntegrator.Producer.Tests/
COPY ./test/OpenFTTH.GDBIntegrator.GeoDatabase.Tests/*.csproj ./test/OpenFTTH.GDBIntegrator.GeoDatabase.Tests/
COPY ./test/OpenFTTH.GDBIntegrator.Integrator.Tests/*.csproj ./test/OpenFTTH.GDBIntegrator.Integrator.Tests/

RUN dotnet restore --packages ./packages

COPY . ./
WORKDIR /app/src/OpenFTTH.GDBIntegrator
RUN dotnet publish -c Release -o out --packages ./packages

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app

COPY --from=build-env /app/src/OpenFTTH.GDBIntegrator/out .
ENTRYPOINT ["dotnet", "OpenFTTH.GDBIntegrator.dll"]
