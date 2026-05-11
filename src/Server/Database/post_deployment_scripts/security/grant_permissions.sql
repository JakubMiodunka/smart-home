PRINT '[INFO] Granting permissions: PermissionsType=[EXECUTE], UserName=[$(APP_USER_NAME)]';

IF EXISTS (SELECT * FROM sys.database_principals WHERE name = '$(APP_USER_NAME)')
BEGIN
    GRANT EXECUTE TO [$(APP_USER_NAME)];
    
    PRINT '[INFO] Permissions granted successfully: PermissionsType=[EXECUTE], UserName=[$(APP_USER_NAME)]';
END
ELSE
BEGIN
    PRINT '[WARNING] Failed to grant permissions: Message=[User not found.], PermissionsType=[EXECUTE], UserName=[$(APP_USER_NAME)]';
END
GO