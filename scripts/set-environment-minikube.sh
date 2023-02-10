#!/bin/bash

# Environment variables

# Application
export APPLICATION__APPLICATIONNAME="GDB_INTEGRATOR"
export APPLICATION__TOLERANCE="0.01"
export APPLICATION__SENDGEOGRAPHICALAREAUPDATEDNOTIFICATION="true"
export APPLICATION__ENABLESEGMENTENDSAUTOSNAPPINGTOROUTENODE="true"
export APPLICATION__APIGATEWAYHOST="http://api-gateway.openftth.local"

# Postgres
export POSTGIS__HOST=$(minikube ip)
export POSTGIS__PORT=$(kubectl describe service openftth-postgis -n openftth | grep NodePort | grep -o '[0-9]\+')
export POSTGIS__DATABASE="OPEN_FTTH"
export POSTGIS__USERNAME="postgres"
export POSTGIS__PASSWORD="postgres"

# Kafka
export KAFKA__SERVER=$(minikube ip):$(kubectl describe service openftth-kafka-cluster-kafka-external-bootstrap -n openftth | grep NodePort | grep -o '[0-9]\+')
export KAFKA__POSTGISROUTENETWORKTOPIC="postgres-connector.route-network"
export KAFKA__POSTGISROUTENETWORKCONSUMER="postgres-connector-gdb-integrator-consumer"

# Notification server
export NOTIFICATIONSERVER__DOMAIN="localhost"
export NOTIFICATIONSERVER__PORT="6666"

# Event store
export EVENTSTORE__CONNECTIONSTRING="Host=event-store.openftth.local;Port=5432;Username=postgres;Password=postgres;Database=EVENT_STORE"

# Logging
export SERILOG__MINIMUMLEVEL="Information"
