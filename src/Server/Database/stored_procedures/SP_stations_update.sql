CREATE PROCEDURE SP_stations_update
    @id BIGINT,  -- Specifies which station shall be updated.
    @update_ip_address BIT = 0,
    @ip_address VARCHAR(39) = NULL,
    @update_last_heartbeat BIT = 0,
    @last_heartbeat DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE stations
    SET 
        ip_address = CASE WHEN @update_ip_address = 1 THEN @ip_address ELSE ip_address END,
        last_heartbeat = CASE WHEN @update_last_heartbeat = 1 THEN @last_heartbeat ELSE last_heartbeat END
    OUTPUT 
        INSERTED.id,
        INSERTED.mac_address,
        INSERTED.ip_address,
        INSERTED.last_heartbeat
    WHERE 
        id = @id
END