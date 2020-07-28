/*
 * Author: jesper@dax.dk
 * Purpose: Tables (route_node and route_segment) used for editing and visualization of the route network in QGIS
 * along with some tables holding information (*_info tables) comming from other sources that can be related to
 * route_node and route_segment tables.
 */

-- Enable PostGIS on database if not already enabled
CREATE EXTENSION IF NOT EXISTS postgis;

CREATE SCHEMA IF NOT EXISTS route_network;

----------------------------------------------------------------------------------
-- User Route node table
----------------------------------------------------------------------------------

-- Create the route node table
CREATE TABLE route_network.route_node
(
	mrid uuid,
	coord geometry(Point,25832),
	node_name varchar(255),
	node_kind varchar(255),
	node_function varchar(255),
	marked_to_be_deleted boolean,
	delete_me boolean,
	work_task_mrid uuid,
	user_name varchar(255),
	application_name varchar(255),
	application_info varchar,
	PRIMARY KEY(mrid)
);

-- Create spatial index
CREATE INDEX route_node_coord_idx
    ON route_network.route_node USING gist ("coord");

-- Create delete trigger that mark route node as deleted (instead of deleting it)
CREATE FUNCTION route_network.route_node_delete() RETURNS TRIGGER AS $_$
BEGIN
    IF NEW.delete_me = false
	THEN
       UPDATE route_network.route_node SET marked_to_be_deleted = true WHERE route_node.mrid = OLD.mrid;
       RETURN null;
	END IF;

	RETURN NEW;
END $_$ LANGUAGE 'plpgsql';

CREATE TRIGGER delete_route_node BEFORE DELETE ON route_network.route_node
FOR EACH ROW
WHEN (pg_trigger_depth() < 1)
EXECUTE PROCEDURE route_network.route_node_delete();


-- Create update trigger to support permanently deleting the node
CREATE FUNCTION route_network.route_node_update() RETURNS TRIGGER AS $_$
BEGIN
    IF NEW.delete_me = true
	THEN
	    DELETE FROM route_network.route_node where mrid = OLD.mrid;
		RETURN null;
	END IF;

	RETURN NEW;
END $_$ LANGUAGE 'plpgsql';

CREATE TRIGGER update_route_node BEFORE UPDATE ON route_network.route_node
FOR EACH ROW EXECUTE PROCEDURE route_network.route_node_update();


----------------------------------------------------------------------------------
-- User Route segment table
----------------------------------------------------------------------------------

-- Create the route segment table
CREATE TABLE route_network.route_segment
(
	mrid uuid,
	coord geometry(Linestring,25832),
	marked_to_be_deleted boolean,
	delete_me boolean,
	segment_kind varchar(255),
	work_task_mrid uuid,
	user_name varchar(255),
	application_name varchar(255),
	application_info varchar,
	PRIMARY KEY(mrid)
);

-- Create spatial index
CREATE INDEX route_segment_coord_idx
    ON route_network.route_segment USING gist ("coord");


-- Create delete trigger that mark route segment as deleted (instead of deleting it)
CREATE FUNCTION route_network.route_segment_delete() RETURNS TRIGGER AS $_$
BEGIN
    IF NEW.delete_me = false
	THEN
       UPDATE route_network.route_segment SET marked_to_be_deleted = true WHERE route_segment.mrid = OLD.mrid;
       RETURN null;
    END IF;

	RETURN NEW;
END $_$ LANGUAGE 'plpgsql';

CREATE TRIGGER delete_route_segment BEFORE DELETE ON route_network.route_segment
FOR EACH ROW
WHEN (pg_trigger_depth() < 1)
EXECUTE PROCEDURE route_network.route_segment_delete();


-- Create update trigger to support permanently deleting the segment
CREATE FUNCTION route_network.route_segment_update() RETURNS TRIGGER AS $_$
BEGIN
    IF NEW.delete_me = true
	THEN
	    DELETE FROM route_network.route_segment where mrid = OLD.mrid;
		RETURN null;
	END IF;

	RETURN NEW;
END $_$ LANGUAGE 'plpgsql';

CREATE TRIGGER update_route_node BEFORE UPDATE ON route_network.route_segment
FOR EACH ROW EXECUTE PROCEDURE route_network.route_segment_update();



CREATE SCHEMA IF NOT EXISTS route_network_integrator;

----------------------------------------------------------------------------------
-- Integrator Route node table
----------------------------------------------------------------------------------

-- Create the route node table
CREATE TABLE route_network_integrator.route_node
(
	mrid uuid,
	coord geometry(Point,25832),
	node_name varchar(255),
	node_kind varchar(255),
	node_function varchar(255),
	marked_to_be_deleted boolean,
	delete_me boolean,
	work_task_mrid uuid,
	user_name varchar(255),
	application_name varchar(255),
	application_info varchar,
	PRIMARY KEY(mrid)
);

-- Create spatial index
CREATE INDEX route_node_coord_idx
    ON route_network_integrator.route_node USING gist ("coord");

-- Create delete trigger that mark route node as deleted (instead of deleting it)
CREATE FUNCTION route_network_integrator.route_node_delete() RETURNS TRIGGER AS $_$
BEGIN
    IF NEW.delete_me = false
    THEN
       UPDATE route_network_integrator.route_node SET marked_to_be_deleted = true WHERE route_node.mrid = OLD.mrid;
       RETURN null;
    END IF;

    RETURN NEW;
END $_$ LANGUAGE 'plpgsql';

CREATE TRIGGER delete_route_node BEFORE DELETE ON route_network_integrator.route_node
FOR EACH ROW
WHEN (pg_trigger_depth() < 1)
EXECUTE PROCEDURE route_network_integrator.route_node_delete();


-- Create update trigger to support permanently deleting the node
CREATE FUNCTION route_network_integrator.route_node_update() RETURNS TRIGGER AS $_$
BEGIN
    IF NEW.delete_me = true
    THEN
        DELETE FROM route_network_integrator.route_node where mrid = OLD.mrid;
        RETURN null;
    END IF;

    RETURN NEW;
END $_$ LANGUAGE 'plpgsql';

CREATE TRIGGER update_route_node BEFORE UPDATE ON route_network_integrator.route_node
FOR EACH ROW EXECUTE PROCEDURE route_network_integrator.route_node_update();


----------------------------------------------------------------------------------
-- Integrator Route segment table
----------------------------------------------------------------------------------

-- Create the route segment table
CREATE TABLE route_network_integrator.route_segment
(
	mrid uuid,
	coord geometry(Linestring,25832),
	marked_to_be_deleted boolean,
	delete_me boolean,
	segment_kind varchar(255),
	work_task_mrid uuid,
	user_name varchar(255),
	application_name varchar(255),
	application_info varchar,
	PRIMARY KEY(mrid)
);

-- Create spatial index
CREATE INDEX route_segment_coord_idx
    ON route_network_integrator.route_segment USING gist ("coord");


-- Create delete trigger that mark route segment as deleted (instead of deleting it)
CREATE FUNCTION route_network_integrator.route_segment_delete() RETURNS TRIGGER AS $_$
BEGIN
    IF NEW.delete_me = false
    THEN
       UPDATE route_network_integrator.route_segment SET marked_to_be_deleted = true WHERE route_segment.mrid = OLD.mrid;
       RETURN null;
    END IF;

    RETURN NEW;
END $_$ LANGUAGE 'plpgsql';

CREATE TRIGGER delete_route_segment BEFORE DELETE ON route_network_integrator.route_segment
FOR EACH ROW
WHEN (pg_trigger_depth() < 1)
EXECUTE PROCEDURE route_network_integrator.route_segment_delete();


-- Create update trigger to support permanently deleting the segment
CREATE FUNCTION route_network_integrator.route_segment_update() RETURNS TRIGGER AS $_$
BEGIN
    IF NEW.delete_me = true
    THEN
        DELETE FROM route_network_integrator.route_segment where mrid = OLD.mrid;
        RETURN null;
        END IF;

    RETURN NEW;
END $_$ LANGUAGE 'plpgsql';

CREATE TRIGGER update_route_node BEFORE UPDATE ON route_network_integrator.route_segment
FOR EACH ROW EXECUTE PROCEDURE route_network_integrator.route_segment_update();
