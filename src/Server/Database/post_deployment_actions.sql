/*
 * In Visual Studio (at least in version 2026), SQLCMD syntax (ex. :r) 
 * may be highlighted as syntax errors.
 * This is a known issue with the T-SQL editor's IntelliSense, which does not 
 * fully parse SQLCMD syntax in offline mode. 
 * The project will build and publish without any issues as these commands are 
 * processed correctly by the SqlPackage/Dacpac engine.
*/

:r .\security\users\app_user.sql
:r .\security\permissions\app_user_permissions.sql