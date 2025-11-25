#!/usr/bin/env bash
set -euo pipefail

API_KEY="supersecret"
BAGET_PORT=5555

sudo apt-get update -y
sudo apt-get install -y apt-transport-https ca-certificates gnupg curl lsb-release docker.io

sudo systemctl enable --now docker

if ! command -v dotnet >/dev/null 2>&1; then
  wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  rm packages-microsoft-prod.deb
  sudo apt-get update -y
  sudo apt-get install -y dotnet-sdk-8.0
fi

sudo docker rm -f baget || true
sudo docker pull loicsharma/baget:latest

sudo docker run -d --restart unless-stopped --name baget \
  -e ApiKey="$API_KEY" \
  -e ASPNETCORE_URLS="http://0.0.0.0:$BAGET_PORT" \
  -e Storage__Type=FileSystem \
  -e Storage__Path=/var/baget/packages \
  -e Database__Type=Sqlite \
  -e Database__ConnectionString="Data Source=/var/baget/baget.db" \
  -p ${BAGET_PORT}:${BAGET_PORT} \
  loicsharma/baget:latest

PACKAGE_PATH=$(ls /vagrant/artifacts/nupkg/EventBudgetPlanner.Cli.Tool.*.nupkg 2>/dev/null || true)
if [ -z "$PACKAGE_PATH" ]; then
  echo "Package not found in /vagrant/artifacts/nupkg. Run scripts/pack-tool.sh before provisioning." >&2
  exit 1
fi

until curl -s "http://localhost:${BAGET_PORT}/health" >/dev/null; do
  echo "Waiting for BaGet to start..."
  sleep 3
done

dotnet nuget push "$PACKAGE_PATH" \
  --source "http://localhost:${BAGET_PORT}/v3/index.json" \
  --api-key "$API_KEY" \
  --skip-duplicate

echo "Package pushed to BaGet."