##
# OpenFTTH GDB Integrator
#
# @file
# @version 0.1

build:
	dotnet build
build-release:
	dotnet build -c Release
clean:
	dotnet clean
restore:
	dotnet restore
start:
	dotnet run -p src/OpenFTTH.GDBIntegrator/OpenFTTH.GDBIntegrator.csproj
test:
	dotnet test
test-verbose:
	dotnet test /p:CollectCoverage=true -l "console;verbosity=detailed"
.PHONY: build build-release clean restore start test

# end
