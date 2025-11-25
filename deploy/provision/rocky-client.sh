#!/usr/bin/env bash
set -euo pipefail
BAGET_URL="http://192.168.56.10:5555/v3/index.json"
PACKAGE_ID="EventBudgetPlanner.Cli.Tool"
PACKAGE_VERSION="1.0.0"

sudo dnf install -y dotnet-sdk-8.0 || {
  sudo rpm -Uvh https://packages.microsoft.com/config/centos/8/packages-microsoft-prod.rpm
  sudo dnf install -y dotnet-sdk-8.0
}

export PATH="$HOME/.dotnet/tools:$PATH"

if dotnet nuget list source | grep -q baget; then
  dotnet nuget remove source baget
fi

dotnet nuget add source "$BAGET_URL" --name baget

if dotnet tool list -g | grep -q event-budget; then
  dotnet tool update -g "$PACKAGE_ID" --version "$PACKAGE_VERSION" --add-source "$BAGET_URL"
else
  dotnet tool install -g "$PACKAGE_ID" --version "$PACKAGE_VERSION" --add-source "$BAGET_URL"
fi

event-budget summary portfolio || true
