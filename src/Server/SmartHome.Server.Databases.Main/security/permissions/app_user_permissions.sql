PRINT '[INFO] Granting permissions: PermissionsType=[EXECUTE], UserName=[$(AppUserName)]';

IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = '$(AppUserName)')
BEGIN
    GRANT EXECUTE TO [$(AppUserName)];
    
    PRINT '[INFO] Permissions granted successfully: PermissionsType=[EXECUTE], UserName=[$(AppUserName)]';
END
ELSE
BEGIN
    PRINT '[WARNING] Failed to grant permissions: Message=[User not found.], PermissionsType=[EXECUTE], UserName=[$(AppUserName)]';
END
GO