ALTER TABLE route_network.route_node ADD CONSTRAINT CHECK_IS_SIMPLE_GEOMETRY CHECK (st_issimple(coord));
ALTER TABLE route_network.route_segment ADD CONSTRAINT CHECK_IS_SIMPLE_GEOMETRY CHECK (st_issimple(coord));
