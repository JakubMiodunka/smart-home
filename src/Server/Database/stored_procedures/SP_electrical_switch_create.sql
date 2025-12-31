CREATE PROCEDURE SP_electrical_switch_create
	@station_identifier BIGINT,
    @local_identifier TINYINT,
	@alias NVARCHAR(100),
	@is_closed BIT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO electrical_switches(
		station_identifier,
		local_identifier,
		alias,
		is_closed)
	OUTPUT
		INSERTED.identifier,
		INSERTED.station_identifier,
		INSERTED.local_identifier,
		INSERTED.alias,
		INSERTED.is_closed
	VALUES(
		@station_identifier,
		@local_identifier,
		@alias,
		@is_closed)
END