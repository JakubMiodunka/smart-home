CREATE TABLE stations
(
	identifier BIGINT IDENTITY(1, 1),
    mac_address CHAR(12) NOT NULL,  -- MAC adress in hexadecimal format without separators.
    ip_address VARCHAR(39) NULL,
    alias NVARCHAR(100) NULL,
    CONSTRAINT PK_stations PRIMARY KEY (identifier),
    CONSTRAINT UQ_stations_mac_address UNIQUE (mac_address)
)
