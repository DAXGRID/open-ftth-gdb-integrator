-- Edit operation table
CREATE TABLE route_network.route_network_edit_operation (
	seq_no BIGINT NOT NULL GENERATED ALWAYS AS IDENTITY,
	event_id UUID NULL,
	"before" VARCHAR NULL,
	"after" VARCHAR NULL,
	"type" VARCHAR NOT NULL,
  event_timestamp TIMESTAMPTZ NOT NULL DEFAULT NOW(),
	CONSTRAINT route_network_edit_operation_pkey PRIMARY KEY (seq_no)
);

-- Trigger create route node
CREATE OR REPLACE FUNCTION route_network.route_node_create()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
BEGIN
	insert into route_network.route_network_edit_operation (event_id, before, after, type)
	values (
		uuid_generate_v4(),
		null,
		to_json(NEW),
    'RouteNode'
	);
    RETURN NEW;
END $function$
;

-- This is a new trigger, so we have to set it.
CREATE TRIGGER create_route_node BEFORE INSERT ON route_network.route_node
FOR EACH ROW EXECUTE PROCEDURE route_network.route_node_create();

-- Trigger update route node
CREATE OR REPLACE FUNCTION route_network.route_node_update()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
BEGIN
    IF NEW.delete_me = true
    THEN
        DELETE FROM route_network.route_node where mrid = OLD.mrid;
        RETURN null;
    END IF;

	insert into route_network.route_network_edit_operation (event_id, before, after, type)
	values (
		uuid_generate_v4(),
		to_json(OLD),
		to_json(NEW),
    'RouteNode'
	);

    RETURN NEW;
END $function$
;

-- Trigger create route segment
CREATE OR REPLACE FUNCTION route_network.route_segment_create()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
BEGIN
	insert into route_network.route_network_edit_operation (event_id, before, after, type)
	values (
		uuid_generate_v4(),
		null,
		to_json(NEW),
    'RouteSegment'
	);
    RETURN NEW;
END $function$
;

-- This is a new trigger, so we have to set it.
CREATE TRIGGER create_route_segment BEFORE INSERT ON route_network.route_segment
FOR EACH ROW EXECUTE PROCEDURE route_network.route_segment_create();

-- Trigger update route segment
CREATE OR REPLACE FUNCTION route_network.route_segment_update()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
BEGIN
    IF NEW.delete_me = true
    THEN
        DELETE FROM route_network.route_segment where mrid = OLD.mrid;
        RETURN null;
    END IF;

	insert into route_network.route_network_edit_operation (event_id, before, after, type)
	values (
		uuid_generate_v4(),
		to_json(OLD),
		to_json(NEW),
    'RouteSegment'
	);

    RETURN NEW;
END $function$
;
