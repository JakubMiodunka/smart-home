CREATE PROCEDURE SP_switches_create
	@station_id BIGINT,
    @local_id TINYINT,
	@expected_state BIT,
	@actual_state BIT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO switches(
		station_id,
		local_id,
		expected_state,
		actual_state)
	OUTPUT
		INSERTED.id,
		INSERTED.station_id,
		INSERTED.local_id,
		INSERTED.expected_state,
		INSERTED.actual_state
	VALUES(
		@station_id,
		@local_id,
		@expected_state,
		@actual_state)
END