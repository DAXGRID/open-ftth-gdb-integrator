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
export KAFKA__EVENTROUTENETWORKTOPICNAME="domain.route-network"
export KAFKA__EVENTGEOGRAPHICALAREAUPDATED="notification.geographical-area-updated"

# Notification server
export NOTIFICATIONSERVER__DOMAIN="notification-server"
export NOTIFICATIONSERVER__PORT="80"

# Logging
export SERILOG__MINIMUMLEVEL="Information"
