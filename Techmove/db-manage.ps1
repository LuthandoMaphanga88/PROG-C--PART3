#!/usr/bin/env pwsh

<#
    Techmove Database Management Helper Script
    This script provides common database operations using PowerShell
#>

param(
    [Parameter(Position = 0)]
    [string]$Command,

    [Parameter(Position = 1)]
    [string]$MigrationName
)

function Show-Help {
    Write-Host @"

Techmove Database Management Helper
====================================

Usage: ./db-manage.ps1 [-Command] <command> [MigrationName]

Commands:
  update              - Apply pending migrations to the database
  add-migration       - Create a new migration with the specified name
  remove-migration    - Remove the last migration (only if not applied)
  reset               - Delete and recreate the database
  help                - Show this help message

Examples:
  ./db-manage.ps1 update
  ./db-manage.ps1 add-migration AddNewTable
  ./db-manage.ps1 reset

"@
}

function Invoke-Update {
    Write-Host "Applying database migrations..." -ForegroundColor Cyan
    dotnet ef database update
}

function Invoke-AddMigration {
    if ([string]::IsNullOrWhiteSpace($MigrationName)) {
        Write-Host "Error: Migration name required" -ForegroundColor Red
        Write-Host "Usage: ./db-manage.ps1 add-migration MigrationName" -ForegroundColor Yellow
        return
    }

    Write-Host "Creating migration: $MigrationName" -ForegroundColor Cyan
    dotnet ef migrations add $MigrationName
}

function Invoke-RemoveMigration {
    Write-Host "Removing last migration (only if not applied to database)..." -ForegroundColor Cyan
    dotnet ef migrations remove
}

function Invoke-Reset {
    Write-Host "WARNING: This will delete and recreate the database!" -ForegroundColor Red
    Write-Host "Press Enter to continue or Ctrl+C to cancel..."
    Read-Host

    Write-Host "Removing database..." -ForegroundColor Cyan
    dotnet ef database drop --force

    Write-Host "Applying migrations..." -ForegroundColor Cyan
    dotnet ef database update

    Write-Host "Database reset complete!" -ForegroundColor Green
}

# Main logic
switch ($Command.ToLower()) {
    "update" {
        Invoke-Update
    }
    "add-migration" {
        Invoke-AddMigration
    }
    "remove-migration" {
        Invoke-RemoveMigration
    }
    "reset" {
        Invoke-Reset
    }
    "help" {
        Show-Help
    }
    "" {
        Show-Help
    }
    default {
        Write-Host "Unknown command: $Command" -ForegroundColor Red
        Show-Help
    }
}
