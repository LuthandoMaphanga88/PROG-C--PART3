@echo off
REM Database Management Helper Script
REM This script provides common database operations

if "%1"=="" goto help
if "%1"=="update" goto update
if "%1"=="add-migration" goto add_migration
if "%1"=="remove-migration" goto remove_migration
if "%1"=="reset" goto reset
if "%1"=="help" goto help
goto help

:update
echo Applying database migrations...
dotnet ef database update
goto end

:add_migration
if "%2"=="" (
    echo Error: Migration name required
    echo Usage: db-manage.bat add-migration MigrationName
    goto end
)
echo Creating migration: %2
dotnet ef migrations add %2
goto end

:remove_migration
echo Removing last migration (only if not applied to database)...
dotnet ef migrations remove
goto end

:reset
echo WARNING: This will delete and recreate the database!
pause
echo Removing database...
dotnet ef database drop --force
echo Applying migrations...
dotnet ef database update
echo Database reset complete!
goto end

:help
echo.
echo Techmove Database Management Helper
echo ====================================
echo.
echo Usage: db-manage.bat [command]
echo.
echo Commands:
echo   update              - Apply pending migrations to the database
echo   add-migration NAME  - Create a new migration with the specified name
echo   remove-migration    - Remove the last migration (only if not applied)
echo   reset               - Delete and recreate the database
echo   help                - Show this help message
echo.
echo Examples:
echo   db-manage.bat update
echo   db-manage.bat add-migration AddNewTable
echo   db-manage.bat reset
echo.

:end
