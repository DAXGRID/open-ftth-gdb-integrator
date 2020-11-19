ALTER TABLE route_network.route_node ADD COLUMN lifecycle_documentation_state VARCHAR(50);
ALTER TABLE route_network.route_segment ADD COLUMN lifecycle_documentation_state VARCHAR(50);

CREATE INDEX idx_route_node_work_task_mrid ON route_network.route_node(work_task_mrid);
CREATE INDEX idx_route_segment_work_task_mrid ON route_network.route_segment(work_task_mrid);

CREATE VIEW route_network.route_segment_survey_import AS
    SELECT *
    FROM route_network.route_segment
    WHERE "marked_to_be_deleted" = false and "work_task_mrid" = 'ed0cb03f-b11c-40a3-8503-9eee2cfaf65c';

CREATE VIEW route_network.route_node_survey_import AS
    SELECT *
    FROM route_network.route_node
    WHERE "marked_to_be_deleted" = false and "work_task_mrid" = 'ed0cb03f-b11c-40a3-8503-9eee2cfaf65c';
