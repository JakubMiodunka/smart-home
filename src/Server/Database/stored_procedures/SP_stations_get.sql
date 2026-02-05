CREATE PROCEDURE SP_stations_get
    @filter_by_id BIT = 0,
    @id BIGINT = NULL,
    @filter_by_ip_address BIT = 0,
    @ip_address VARCHAR(39) = NULL,
    @filter_by_mac_address BIT = 0,
    @mac_address CHAR(12) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        id,
        mac_address,
        ip_address,
        last_heartbeat
    FROM stations
    WHERE
        (@filter_by_id = 0 OR id IS NOT DISTINCT FROM @id) AND
        (@filter_by_ip_address = 0 OR ip_address IS NOT DISTINCT FROM @ip_address) AND
        (@filter_by_mac_address = 0 OR mac_address IS NOT DISTINCT FROM @mac_address)
    ORDER BY id ASC
END