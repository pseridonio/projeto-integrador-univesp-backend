<#
Helper script to run the API with optional migration argument.
Usage:
  ./run.ps1           -> runs without applying migrations
  ./run.ps1 -Migrate  -> applies migrations before running
#>
param(
    [switch]$Migrate
)

if ($Migrate) {
    dotnet run --project "app/CafeSystem.API" -- --migrate
} else {
    dotnet run --project "app/CafeSystem.API"
}
