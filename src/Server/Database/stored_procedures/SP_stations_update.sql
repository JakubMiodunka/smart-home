CREATE PROCEDURE SP_stations_update
    @mac_address CHAR(12),
    @ip_address VARCHAR(39) = NULL,
    @ignore_ip_address BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE stations
    SET 
        ip_address = CASE WHEN @ignore_ip_address = 0 THEN @ip_address ELSE ip_address END
    OUTPUT 
        INSERTED.id,
        INSERTED.mac_address,
        INSERTED.ip_address
    WHERE 
        mac_address = @mac_address
END