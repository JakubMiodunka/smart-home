CREATE TABLE stations
(
	id BIGINT IDENTITY(1, 1),
    mac_address CHAR(12) NOT NULL,  -- MAC adress in hexadecimal format without separators.
    ip_address VARCHAR(39) NULL,    -- IPv4 or IPv6 adress with separators.
    CONSTRAINT PK_stations PRIMARY KEY (id),
    CONSTRAINT UQ_stations_mac_address UNIQUE (mac_address)
)
