CREATE PROCEDURE sensors_create
	@station_id BIGINT,
    @local_id TINYINT,
	@measurement_type TINYINT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO sensors(
		station_id,
		local_id,
		measurement_type)
	OUTPUT
		INSERTED.id,
		INSERTED.station_id,
		INSERTED.local_id,
		INSERTED.measurement_type
	VALUES(
		@station_id,
		@local_id,
		@measurement_type)
END
GO
