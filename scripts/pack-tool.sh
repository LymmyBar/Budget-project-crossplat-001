#!/usr/bin/env bash
set -euo pipefail
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")"/.. && pwd)"
OUTPUT_DIR="$ROOT_DIR/artifacts/nupkg"
mkdir -p "$OUTPUT_DIR"
dotnet pack "$ROOT_DIR/src/EventBudgetPlanner.Cli/EventBudgetPlanner.Cli.csproj" -c Release -o "$OUTPUT_DIR"
echo "NuGet package generated in $OUTPUT_DIR"
