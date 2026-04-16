CREATE TABLE stations
(
	id BIGINT IDENTITY(1, 1),
    mac_address CHAR(12) NOT NULL,  -- MAC adress in hexadecimal format without separators.
    ip_address VARCHAR(39) NULL,    -- IPv4 or IPv6 adress with separators.
    api_port INT NULL,
    api_version TINYINT NULL,
    last_heartbeat DATETIMEOFFSET NOT NULL,
    CONSTRAINT PK_stations PRIMARY KEY (id),
    CONSTRAINT UQ_stations_mac_address UNIQUE (mac_address),
    CONSTRAINT UQ_stations_ip_address UNIQUE (ip_address),
    CONSTRAINT CK_api_port CHECK (api_port IS NULL OR (0 <= api_port AND api_port <= 65535))
)
