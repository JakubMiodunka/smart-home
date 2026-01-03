CREATE PROCEDURE SP_stations_update
    @mac_address CHAR(12),  -- Specifies which station shall be updated.
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
        mac_address = @mac_address
END