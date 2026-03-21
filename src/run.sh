#!/usr/bin/env bash
# Helper script to run the API with optional migration argument.
# Usage:
#   ./run.sh           -> runs without applying migrations
#   ./run.sh migrate   -> applies migrations before running

if [[ "$1" == "migrate" ]]; then
  dotnet run --project "app/CafeSystem.API" -- --migrate
else
  dotnet run --project "app/CafeSystem.API"
fi
