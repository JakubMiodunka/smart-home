CREATE PROCEDURE SP_electrical_switches_get
    @filter_by_id BIT = 0,
    @id BIGINT = NULL,
    @filter_by_station_id BIT = 0,
    @station_id BIGINT = NULL
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
        -- TODO Something is wrong with this filtering logic...
        (@filter_by_id = 1 AND id = @id) OR
        (@filter_by_station_id = 1 AND station_id = @station_id)
    ORDER BY id ASC
END