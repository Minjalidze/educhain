#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LOG_DIR="$ROOT_DIR/.local/logs"
PID_DIR="$ROOT_DIR/.local/pids"
mkdir -p "$LOG_DIR" "$PID_DIR"

POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-postgres}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_DB="${POSTGRES_DB:-document_verifier}"
BACKEND_PORT="${BACKEND_PORT:-5000}"
FRONTEND_PORT="${FRONTEND_PORT:-3000}"
HARDHAT_PORT="${HARDHAT_PORT:-8545}"
JWT_SIGNING_KEY="${JWT_SIGNING_KEY:-dev-only-change-this-signing-key-32chars}"

# Public Hardhat local private key. Use only with the local Hardhat network.
HARDHAT_PRIVATE_KEY="${HARDHAT_PRIVATE_KEY:-0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80}"

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Missing command: $1"
    echo "Install dependencies for Ubuntu 24.04: sudo apt update && sudo apt install -y docker.io docker-compose-plugin nodejs npm"
    echo "Install .NET SDK 10 from Microsoft packages if dotnet is missing."
    exit 1
  fi
}

wait_for_url() {
  local url="$1"
  local name="$2"
  for _ in {1..60}; do
    if curl -fsS "$url" >/dev/null 2>&1; then
      echo "$name is ready: $url"
      return 0
    fi
    sleep 1
  done
  echo "$name did not become ready: $url"
  exit 1
}

cleanup_old_process() {
  local pid_file="$1"
  if [[ -f "$pid_file" ]]; then
    local pid
    pid="$(cat "$pid_file")"
    if [[ -n "$pid" ]] && kill -0 "$pid" >/dev/null 2>&1; then
      kill "$pid" >/dev/null 2>&1 || true
      sleep 1
    fi
    rm -f "$pid_file"
  fi
}

require_command docker
require_command npm
require_command node
require_command dotnet
require_command curl

if ! docker compose version >/dev/null 2>&1; then
  echo "Docker Compose plugin is required."
  exit 1
fi

echo "Starting PostgreSQL..."
docker compose -f "$ROOT_DIR/infra/docker-compose.yml" up -d postgres

echo "Waiting for PostgreSQL..."
for _ in {1..60}; do
  if docker exec document-verifier-postgres pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB" >/dev/null 2>&1; then
    break
  fi
  sleep 1
done

echo "Installing blockchain dependencies..."
cd "$ROOT_DIR/blockchain"
if [[ ! -d node_modules ]]; then
  npm install
fi

cleanup_old_process "$PID_DIR/hardhat.pid"
echo "Starting Hardhat node..."
npm run node >"$LOG_DIR/hardhat.log" 2>&1 &
echo $! >"$PID_DIR/hardhat.pid"
wait_for_url "http://127.0.0.1:$HARDHAT_PORT" "Hardhat RPC"

echo "Deploying smart contract..."
npm run deploy:local >"$LOG_DIR/deploy.log" 2>&1
CONTRACT_ADDRESS="$(node -p "require('./deployments/localhost.json').contractAddress")"
echo "Contract address: $CONTRACT_ADDRESS"

echo "Restoring and building backend..."
cd "$ROOT_DIR/backend"
dotnet restore DocumentVerifier.sln
dotnet build DocumentVerifier.sln

cleanup_old_process "$PID_DIR/backend.pid"
echo "Starting backend..."
ASPNETCORE_URLS="http://localhost:$BACKEND_PORT" \
ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" \
Jwt__SigningKey="$JWT_SIGNING_KEY" \
Cors__FrontendOrigin="http://localhost:$FRONTEND_PORT" \
Blockchain__UseMock="false" \
Blockchain__RpcUrl="http://127.0.0.1:$HARDHAT_PORT" \
Blockchain__ChainId="31337" \
Blockchain__ContractAddress="$CONTRACT_ADDRESS" \
Blockchain__PrivateKey="$HARDHAT_PRIVATE_KEY" \
Blockchain__NetworkName="hardhat-local" \
dotnet run --project src/DocumentVerifier.Api --no-build >"$LOG_DIR/backend.log" 2>&1 &
echo $! >"$PID_DIR/backend.pid"
wait_for_url "http://localhost:$BACKEND_PORT/api/health" "Backend"

echo "Installing frontend dependencies..."
cd "$ROOT_DIR/frontend/web"
if [[ ! -d node_modules ]]; then
  npm install
fi

cleanup_old_process "$PID_DIR/frontend.pid"
echo "Starting frontend..."
NEXT_PUBLIC_API_URL="http://localhost:$BACKEND_PORT" \
npm run dev -- --port "$FRONTEND_PORT" >"$LOG_DIR/frontend.log" 2>&1 &
echo $! >"$PID_DIR/frontend.pid"
wait_for_url "http://localhost:$FRONTEND_PORT" "Frontend"

cat <<EOF

Local stand is ready.

Frontend: http://localhost:$FRONTEND_PORT
Backend:  http://localhost:$BACKEND_PORT
Swagger:  http://localhost:$BACKEND_PORT/swagger
Postgres: localhost:5432 ($POSTGRES_USER / $POSTGRES_PASSWORD), database $POSTGRES_DB

Admin:
  login:    admin@example.local
  password: admin123

Issuer:
  login:    issuer@example.local
  password: issuer123

Logs:
  $LOG_DIR

Stop:
  kill \$(cat "$PID_DIR/frontend.pid") \$(cat "$PID_DIR/backend.pid") \$(cat "$PID_DIR/hardhat.pid")
  docker compose -f "$ROOT_DIR/infra/docker-compose.yml" down
EOF
