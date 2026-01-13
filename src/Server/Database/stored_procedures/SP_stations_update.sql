/*
    Currently updating station IP address is needed, but method is already prepared
    to support multiple properties update if it will be needed in the future.
*/
CREATE PROCEDURE SP_stations_update
    @id BIGINT,  -- Specifies which station shall be updated.
    @update_ip_address BIT = 0,
    @ip_address VARCHAR(39) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE stations
    SET 
        ip_address = CASE WHEN @update_ip_address = 1 THEN @ip_address ELSE ip_address END
    OUTPUT 
        INSERTED.id,
        INSERTED.mac_address,
        INSERTED.ip_address
    WHERE 
        id = @id
END