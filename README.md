# OPEN-FTTH Geodatabase integrator

## Build status
[![CircleCI](https://circleci.com/gh/DAXGRID/open-ftth-gdb-integrator/tree/master.svg?style=shield&circle)](https://circleci.com/gh/DAXGRID/open-ftth-gdb-integrator/tree/master)

## Configuration
Either rename 'appsettings.example.json' to 'appsettings.json' or make a new
file, after that fill out all the empty fields in the appsettings file.

## Requirements running the application
* gnumake
* dotnet runtime 3.1

### Note
On windows it can be done using chocolatey with the following command:
``` sh
choco install make
```

## Running
Running the application
``` makefile
make start
```
