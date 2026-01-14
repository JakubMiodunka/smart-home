/*
    Currently filtering by ID is needed, but procedure is already prepared
    to support more filtering criteria if it will be needed in the future.
*/
CREATE PROCEDURE SP_electrical_switches_get
    @filter_by_id BIT = 0,
    @id BIGINT = NULL
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
        (@filter_by_id = 0 OR id = @id)
    ORDER BY id ASC
END