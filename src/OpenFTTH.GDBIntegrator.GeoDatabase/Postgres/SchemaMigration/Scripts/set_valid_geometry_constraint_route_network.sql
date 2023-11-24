ALTER TABLE route_network.route_node ADD CONSTRAINT CHECK_GEOMETRY CHECK (st_isvalid(coord));
ALTER TABLE route_network.route_segment ADD CONSTRAINT CHECK_GEOMETRY CHECK (st_isvalid(coord));
