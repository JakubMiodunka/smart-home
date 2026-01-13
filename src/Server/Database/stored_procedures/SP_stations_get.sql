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
        -- TODO Something is wrong with this filtering logic...
        (@filter_by_id = 1 AND id = @id) OR
        (@filter_by_mac_address = 1 AND mac_address = @mac_address)
    ORDER BY id ASC
END