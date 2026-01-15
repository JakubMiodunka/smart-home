CREATE PROCEDURE SP_stations_get
    @filter_by_id BIT = 0,
    @id BIGINT = NULL,
    @filter_by_mac_address BIT = 0,
    @mac_address CHAR(12) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id,
        mac_address,
        ip_address 
    FROM stations
    WHERE
        (@filter_by_id = 0 OR id IS NOT DISTINCT FROM @id) AND
        (@filter_by_mac_address = 0 OR mac_address IS NOT DISTINCT FROM @mac_address)
    ORDER BY id ASC
END