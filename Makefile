##
# OpenFTTH GDB Integrator
#
# @file
# @version 0.1

build:
	dotnet build
clean:
	dotnet clean
restore:
	dotnet restore
start:
	dotnet run -p src/OpenFTTH.GDBIntegrator/OpenFTTH.GDBIntegrator.csproj
test:
	dotnet test

.PHONY: build clean restore start test

# end
