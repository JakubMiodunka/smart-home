PRINT '[INFO] Creating database user: UserName=[$(AppUserName)], UserLogin=[$(AppUserLogin)]';

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$(AppUserName)')
BEGIN
    CREATE USER [$(AppUserName)] FOR LOGIN [$(AppUserLogin)];
    
    PRINT '[INFO] User created successfully: UserName=[$(AppUserName)], UserLogin=[$(AppUserLogin)]';
END
ELSE
BEGIN
    ALTER USER [$(AppUserName)] WITH LOGIN = [$(AppUserLogin)];

    PRINT '[WARNING] Existing user was altered to be associated with specified login: UserName=[$(AppUserName)], UserLogin=[$(AppUserLogin)]';
END
GO