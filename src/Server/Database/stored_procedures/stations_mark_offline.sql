CREATE PROCEDURE stations_mark_offline
    @min_heartbeat_timestamp DATETIMEOFFSET
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @OfflineStations TABLE (id BIGINT);

    INSERT INTO @OfflineStations (id)
    SELECT id
    FROM stations
    WHERE 
        last_heartbeat <= @min_heartbeat_timestamp
        AND ip_address IS NOT NULL
        AND api_port IS NOT NULL
        AND api_version IS NOT NULL;

    IF NOT EXISTS (SELECT 1 FROM @OfflineStations)
    BEGIN
        SELECT id FROM @OfflineStations;    -- To return an empty result set with the correct schema.
        RETURN;
    END

    BEGIN TRANSACTION;
    BEGIN TRY

        UPDATE switches
        SET actual_state = NULL
        WHERE station_id IN (SELECT id FROM @OfflineStations);

        UPDATE stations
        SET
            ip_address = NULL,
            api_port = NULL,
            api_version = NULL
        WHERE id IN (SELECT id FROM @OfflineStations);

        COMMIT TRANSACTION;

        SELECT id FROM @OfflineStations;

    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        THROW;
    END CATCH
END
GO