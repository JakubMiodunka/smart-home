CREATE TABLE measurements
(
	[id] BIGINT IDENTITY(1, 1),
	[sensor_id] BIGINT NOT NULL,
	[value] FLOAT NOT NULL,
	[timestamp] DATETIMEOFFSET NOT NULL,
	CONSTRAINT PK_measurements PRIMARY KEY (id),
	CONSTRAINT FK_measurements_sensors FOREIGN KEY (sensor_id) REFERENCES sensors(id),
)
