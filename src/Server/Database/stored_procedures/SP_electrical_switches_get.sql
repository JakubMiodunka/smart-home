CREATE PROCEDURE SP_electrical_switches_get
    @filter_by_id BIT = 0,
    @id BIGINT = NULL,
    @filter_by_station_id BIT = 0,
    @station_id BIGINT = NULL,
    @filter_by_local_id BIT = 0,
    @local_id TINYINT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
        id,
        station_id,
        local_id,
        is_closed
    FROM electrical_switches
    WHERE
        (@filter_by_id = 0 OR id = @id) AND
        (@filter_by_station_id = 0 OR station_id = @station_id) AND
        (@filter_by_local_id = 0 OR local_id = @local_id)
    ORDER BY id ASC
END