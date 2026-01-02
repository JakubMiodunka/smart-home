CREATE PROCEDURE SP_electrical_switches_create
	@station_id BIGINT,
    @local_id TINYINT,
	@is_closed BIT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO electrical_switches(
		station_id,
		local_id,
		is_closed)
	OUTPUT
		INSERTED.id,
		INSERTED.station_id,
		INSERTED.local_id,
		INSERTED.is_closed
	VALUES(
		@station_id,
		@local_id,
		@is_closed)
END