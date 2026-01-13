CREATE TABLE electrical_switches
(
	id BIGINT IDENTITY(1, 1),
    station_id BIGINT NOT NULL,
    local_id TINYINT NOT NULL,  -- Identifier unique within the station.
    is_closed BIT NULL, -- State of the electrical switch - NULL if switch state is unknown, 1 if the cuirquit is closed and current is flowing, 0 otherwise.
    CONSTRAINT PK_electrical_switches PRIMARY KEY (id),
    CONSTRAINT FK_electrical_switches_stations FOREIGN KEY (station_id) REFERENCES stations(id),
    CONSTRAINT UQ_electrical_switches_station_id_local_id UNIQUE (station_id, local_id)
)
