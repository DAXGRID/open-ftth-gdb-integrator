# OPEN-FTTH Geodatabase integrator

Service that enables route network editing and visualization in QGIS through a geographical-database.

## Build status

[![CircleCI](https://circleci.com/gh/DAXGRID/open-ftth-gdb-integrator/tree/master.svg?style=shield&circle)](https://circleci.com/gh/DAXGRID/open-ftth-gdb-integrator/tree/master)
[![MIT](https://img.shields.io/badge/license-MIT-green.svg?style=flat-square)](./LICENSE)

## Configuration

Configure environment variables using minikube ip and ports.

```sh
. ./scripts/set-environment-minikube.sh
```

## Requirements running the application

* [Taskfile](https://taskfile.dev/#/installation)

## Running

Running the application

``` sh
task start
```

Testing

``` sh
task test
```
