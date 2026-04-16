CREATE TABLE switches
(
	id BIGINT IDENTITY(1, 1),
    station_id BIGINT NOT NULL,
    local_id TINYINT NOT NULL,      -- The identifier of the switch, unique only at the station level.
    expected_state BIT NOT NULL,    -- Expected state of the switch - 1 if the cuirquit shall be closed and current shall flow, 0 otherwise.
    actual_state BIT NULL,          -- Actual state of the switch - NULL if switch state is unknown, 1 if the cuirquit is closed and current is flowing, 0 otherwise.
    CONSTRAINT PK_switches PRIMARY KEY (id),
    CONSTRAINT FK_switches_stations FOREIGN KEY (station_id) REFERENCES stations(id),
    CONSTRAINT UQ_switches_station_id_local_id UNIQUE (station_id, local_id)
)
