CREATE PROCEDURE SP_switches_update
    @id BIGINT,  -- Specifies which electrical switch shall be updated.
    @update_expected_state BIT = 0,
    @expected_state BIT = NULL,
    @update_actual_state BIT = 0,
    @actual_state BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE switches
    SET 
        expected_state = CASE WHEN @update_expected_state = 1 THEN @expected_state ELSE expected_state END,
        actual_state = CASE WHEN @update_actual_state = 1 THEN @actual_state ELSE actual_state END
    OUTPUT 
		INSERTED.id,
		INSERTED.station_id,
		INSERTED.local_id,
		INSERTED.expected_state,
        INSERTED.actual_state
    WHERE 
        id = @id
END