CREATE PROCEDURE stations_mark_as_offline
    @id BIGINT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRANSACTION;
    BEGIN TRY
        
        UPDATE switches
        SET actual_state = NULL
        WHERE station_id = @id;

        UPDATE stations
        SET
            ip_address = NULL,
            api_port = NULL,
            api_version = NULL
        WHERE id = @id;

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO