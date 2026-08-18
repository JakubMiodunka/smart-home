CREATE TABLE sensors
(
	id BIGINT IDENTITY(1, 1),
	station_id BIGINT NOT NULL,
	local_id TINYINT NOT NULL,
	measurement_type TINYINT NOT NULL,
	CONSTRAINT PK_sensors PRIMARY KEY (id),
	CONSTRAINT FK_sensors_stations FOREIGN KEY (station_id) REFERENCES stations(id),
    CONSTRAINT UQ_sensors_station_id_local_id UNIQUE (station_id, local_id)
)
