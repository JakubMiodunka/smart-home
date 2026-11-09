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
    CONSTRAINT CK_api_port CHECK (api_port IS NULL OR (0 <= api_port AND api_port <= 65535))
)
GO

/*
 * MS SQL Server treats NULL as a distinct value in a UNIQUE CONSTRAINT (allowing only a single NULL).
 * To enforce IP address uniqueness while allowing multiple offline stations (NULL values),
 * a filtered non-clustered index must be used.
*/
CREATE UNIQUE NONCLUSTERED INDEX UQ_stations_ip_address 
ON stations (ip_address) 
WHERE ip_address IS NOT NULL;
GO
