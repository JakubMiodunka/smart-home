CREATE TABLE electrical_switches
(
	identifier BIGINT IDENTITY(1, 1),
    station_identifier BIGINT NOT NULL,
    local_identifier TINYINT NOT NULL,  -- Identifier unique within the station.
    alias NVARCHAR(100) NULL,
    is_closed BIT NULL, -- State of the electrical switch - 1 if the cuirquit is closed and current is flowing, 0 otherwise.
    CONSTRAINT PK_electrical_switches PRIMARY KEY (identifier),
    CONSTRAINT FK_electrical_switches_stations FOREIGN KEY (station_identifier) REFERENCES stations(identifier),
    CONSTRAINT UQ_electrical_switches_stations UNIQUE (station_identifier, local_identifier),
)
