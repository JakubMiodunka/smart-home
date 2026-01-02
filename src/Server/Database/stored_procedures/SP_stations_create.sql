CREATE PROCEDURE SP_stations_create
	@mac_address CHAR(12),
    @ip_address VARCHAR(39)
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO stations(
		mac_address,
		ip_address)
	OUTPUT
		INSERTED.id,
		INSERTED.mac_address,
		INSERTED.ip_address
	VALUES(
		@mac_address,
		@ip_address)
END