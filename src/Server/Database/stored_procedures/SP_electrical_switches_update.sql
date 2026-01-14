/*
    Currently updating electrical switch state is needed, but procedure is already prepared
    to support multiple properties update if it will be needed in the future.
*/
CREATE PROCEDURE SP_electrical_switches_update
    @id BIGINT,  -- Specifies which electrical switch shall be updated.
    @update_state BIT = 0,
    @is_closed BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE electrical_switches
    SET 
        is_closed = CASE WHEN @is_closed = 1 THEN @is_closed ELSE is_closed END
    OUTPUT 
		INSERTED.id,
		INSERTED.station_id,
		INSERTED.local_id,
		INSERTED.is_closed
    WHERE 
        id = @id
END