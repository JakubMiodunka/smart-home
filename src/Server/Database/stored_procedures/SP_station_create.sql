CREATE PROCEDURE SP_station_create
	@mac_address CHAR(12),
    @ip_address VARCHAR(39),
	@alias NVARCHAR(100)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO stations(
		mac_address,
		ip_address,
		alias)
	OUTPUT
		INSERTED.identifier,
		INSERTED.mac_address,
		INSERTED.ip_address,
		INSERTED.alias
	VALUES(
		@mac_address,
		@ip_address,
		@alias)
END