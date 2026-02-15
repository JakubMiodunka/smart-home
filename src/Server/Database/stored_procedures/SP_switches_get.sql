CREATE PROCEDURE SP_switches_get
    @filter_by_id BIT = 0,
    @id BIGINT = NULL,
    @filter_by_station_id BIT = 0,
    @station_id BIGINT = NULL,
    @filter_by_local_id BIT = 0,
    @local_id TINYINT = NULL,
    @filter_by_actual_state BIT = 0,
    @actual_state BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
        id,
        station_id,
        local_id,
        expected_state,
        actual_state
    FROM switches
    WHERE
        (@filter_by_id = 0 OR id IS NOT DISTINCT FROM @id) AND
        (@filter_by_station_id = 0 OR station_id IS NOT DISTINCT FROM @station_id) AND
        (@filter_by_local_id = 0 OR local_id IS NOT DISTINCT FROM @local_id) AND
        (@filter_by_actual_state = 0 OR actual_state IS NOT DISTINCT FROM @actual_state)
    ORDER BY id ASC
END