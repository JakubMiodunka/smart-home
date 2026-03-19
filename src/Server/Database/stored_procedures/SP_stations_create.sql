CREATE PROCEDURE SP_stations_create
	@mac_address CHAR(12),
    @ip_address VARCHAR(39),
	@api_port INT,
	@last_heartbeat DATETIME2
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO stations(
		mac_address,
		ip_address,
		api_port,
		last_heartbeat)
	OUTPUT
		INSERTED.id,
		INSERTED.mac_address,
		INSERTED.ip_address,
		INSERTED.api_port,
		INSERTED.last_heartbeat
	VALUES(
		@mac_address,
		@ip_address,
		@api_port,
		@last_heartbeat)
END