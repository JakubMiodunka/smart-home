CREATE PROCEDURE SP_station_exists
	@mac_address CHAR(12)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT CASE 
		WHEN EXISTS (SELECT 1 FROM stations WHERE mac_address = @mac_address)
			THEN CAST(1 AS BIT)
			ELSE CAST(0 AS BIT)
	END AS result
END