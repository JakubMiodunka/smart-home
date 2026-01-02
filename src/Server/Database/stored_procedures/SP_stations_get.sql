CREATE PROCEDURE SP_stations_get
    @mac_address CHAR(12) = NULL,
    @ignore_mac_address BIT = 1
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id,
        mac_address,
        ip_address 
    FROM stations
    WHERE (@ignore_mac_address = 1 OR mac_address = @mac_address)
    ORDER BY id ASC
END