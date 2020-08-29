#!/bin/fish

# Environment variables for fish shell

# Application
export APPLICATION__APPLICATIONNAME="GDB_INTEGRATOR"
export APPLICATION__TOLERANCE="0.1"

# Postgres
export POSTGIS__HOST=(minikube ip)
export POSTGIS__PORT=(kubectl describe service openftth-postgis -n openftth | grep NodePort | grep -o '[0-9]\+')
export POSTGIS__DATABASE="OPEN_FTTH"
export POSTGIS__USERNAME="postgres"
export POSTGIS__PASSWORD="postgres"

# Kafka
export KAFKA__SERVER=(minikube ip):(kubectl describe service openftth-kafka-cluster-kafka-external-bootstrap -n openftth | grep NodePort | grep -o '[0-9]\+')
export KAFKA__POSTGISROUTENETWORKTOPIC="postgis.route-network"
export KAFKA__POSTGISROUTENETWORKCONSUMER="postgis-route-network-consumer"
export KAFKA__POSITIONFILEPATH="/tmp/"
export KAFKA__EVENTROUTENETWORKTOPICNAME="event.route-network"

# Logging
export SERILOG__MINIMUMLEVEL__DEFAULT="Information"
export SERILOG__MINIMUMLEVEL__WRITETO="Console"
