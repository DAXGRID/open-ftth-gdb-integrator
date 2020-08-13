# OPEN-FTTH Geodatabase integrator

## Build status

[![CircleCI](https://circleci.com/gh/DAXGRID/open-ftth-gdb-integrator/tree/master.svg?style=shield&circle)](https://circleci.com/gh/DAXGRID/open-ftth-gdb-integrator/tree/master)
[![MIT](https://img.shields.io/badge/license-MIT-green.svg?style=flat-square)](./LICENSE)

## Configuration

Configure environment variables call (fish shell) using minikube Kubernetes.

```sh
. ./scripts/set-environment-minikube.fish
```

## Requirements running the application

* [Taskfile](https://taskfile.dev/#/installation)
* dotnet runtime 3.1

## Running

Running the application

``` makefile
task start
```

## Node Editing

Info about node editing can be found here [Link](https://github.com/DAXGRID/open-ftth-overview/blob/master/Route%20Network%20Editing%20Details/Overview.md)
