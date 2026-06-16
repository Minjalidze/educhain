#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
LOG_DIR="$ROOT_DIR/.local/logs"
PID_DIR="$ROOT_DIR/.local/pids"
ENV_FILE="$ROOT_DIR/.local/vds.env"

mkdir -p "$LOG_DIR" "$PID_DIR"

PUBLIC_HOST="${PUBLIC_HOST:-186.246.7.98}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-postgres}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_DB="${POSTGRES_DB:-document_verifier}"
BACKEND_PORT="${BACKEND_PORT:-5000}"
FRONTEND_PORT="${FRONTEND_PORT:-3000}"
HARDHAT_PORT="${HARDHAT_PORT:-8545}"
FRONTEND_MODE="${FRONTEND_MODE:-prod}"

PUBLIC_FRONTEND_URL="${PUBLIC_FRONTEND_URL:-http://$PUBLIC_HOST:$FRONTEND_PORT}"
PUBLIC_BACKEND_URL="${PUBLIC_BACKEND_URL:-http://$PUBLIC_HOST:$BACKEND_PORT}"

# Public Hardhat local private key. Use only with the local Hardhat network.
HARDHAT_PRIVATE_KEY="${HARDHAT_PRIVATE_KEY:-0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80}"

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Missing command: $1"
    echo
    echo "Install the required runtime first:"
    echo "  Docker + Compose plugin"
    echo "  Node.js 22+ and npm"
    echo "  .NET SDK 10"
    echo "  curl"
    exit 1
  fi
}

require_node_version() {
  local version major minor patch
  version="$(node -p "process.versions.node")"
  IFS=. read -r major minor patch <<<"$version"

  if (( major < 20 || (major == 20 && minor < 9) )); then
    echo "Node.js $version is installed, but Next.js requires Node.js >= 20.9.0."
    echo
    echo "Install Node.js 22 on Ubuntu:"
    echo "  curl -fsSL https://deb.nodesource.com/setup_22.x | sudo -E bash -"
    echo "  sudo apt install -y nodejs"
    echo
    echo "Then remove stale frontend dependencies and start again:"
    echo "  rm -rf \"$ROOT_DIR/frontend/web/node_modules\""
    echo "  ./scripts/start-vds.sh"
    exit 1
  fi

  echo "Node.js version is OK: $version"
}

generate_jwt_key() {
  if command -v openssl >/dev/null 2>&1; then
    openssl rand -base64 48 | tr -d '\n'
  else
    dd if=/dev/urandom bs=48 count=1 2>/dev/null | base64 | tr -d '\n'
  fi
}

load_or_create_env() {
  if [[ ! -f "$ENV_FILE" ]]; then
    umask 077
    printf 'JWT_SIGNING_KEY=%q\n' "$(generate_jwt_key)" >"$ENV_FILE"
  fi

  # shellcheck disable=SC1090
  source "$ENV_FILE"
  JWT_SIGNING_KEY="${JWT_SIGNING_KEY:-dev-only-change-this-signing-key-32chars}"
}

install_npm_deps() {
  if [[ -d node_modules ]]; then
    return 0
  fi

  if [[ -f package-lock.json ]]; then
    npm ci
  else
    npm install
  fi
}

wait_for_url() {
  local url="$1"
  local name="$2"
  local log_file="${3:-}"
  local pid_file="${4:-}"

  for _ in {1..90}; do
    if curl -fsS "$url" >/dev/null 2>&1; then
      echo "$name is ready: $url"
      return 0
    fi

    if [[ -n "$pid_file" && -f "$pid_file" ]]; then
      local pid
      pid="$(cat "$pid_file")"
      if [[ -n "$pid" ]] && ! kill -0 "$pid" >/dev/null 2>&1; then
        echo "$name process exited before it became ready."
        if [[ -n "$log_file" && -f "$log_file" ]]; then
          echo
          echo "Last $name log lines:"
          tail -n 120 "$log_file"
        fi
        exit 1
      fi
    fi

    sleep 1
  done

  echo "$name did not become ready: $url"
  if [[ -n "$log_file" && -f "$log_file" ]]; then
    echo
    echo "Last $name log lines:"
    tail -n 120 "$log_file"
  fi
  exit 1
}

wait_for_tcp() {
  local host="$1"
  local port="$2"
  local name="$3"

  for _ in {1..90}; do
    if timeout 1 bash -c "cat < /dev/null > /dev/tcp/$host/$port" >/dev/null 2>&1; then
      echo "$name is ready: $host:$port"
      return 0
    fi
    sleep 1
  done

  echo "$name did not become ready: $host:$port"
  exit 1
}

check_cors_preflight() {
  local response
  response="$(
    curl -i -sS -X OPTIONS "http://127.0.0.1:$BACKEND_PORT/api/auth/login" \
      -H "Origin: $PUBLIC_FRONTEND_URL" \
      -H "Access-Control-Request-Method: POST" \
      -H "Access-Control-Request-Headers: content-type" || true
  )"

  if ! grep -qi '^Access-Control-Allow-Origin:' <<<"$response"; then
    echo "Backend CORS preflight failed."
    echo
    echo "$response"
    echo
    echo "Backend log:"
    tail -n 120 "$LOG_DIR/backend.log"
    exit 1
  fi

  echo "Backend CORS preflight is ready for $PUBLIC_FRONTEND_URL."
}

check_frontend_api_proxy() {
  local response
  response="$(curl -fsS "http://127.0.0.1:$FRONTEND_PORT/api/health" || true)"

  if ! grep -q '"status"' <<<"$response"; then
    echo "Frontend API proxy failed."
    echo
    echo "$response"
    echo
    echo "Frontend log:"
    tail -n 120 "$LOG_DIR/frontend.log"
    exit 1
  fi

  echo "Frontend API proxy is ready: http://127.0.0.1:$FRONTEND_PORT/api/health"
}

stop_pid_file() {
  local pid_file="$1"
  local name="$2"

  if [[ ! -f "$pid_file" ]]; then
    return 0
  fi

  local pid
  pid="$(cat "$pid_file")"
  if [[ -n "$pid" ]] && kill -0 "$pid" >/dev/null 2>&1; then
    echo "Stopping $name ($pid)..."
    kill "$pid" >/dev/null 2>&1 || true

    for _ in {1..20}; do
      if ! kill -0 "$pid" >/dev/null 2>&1; then
        break
      fi
      sleep 0.2
    done

    if kill -0 "$pid" >/dev/null 2>&1; then
      kill -9 "$pid" >/dev/null 2>&1 || true
    fi
  fi

  rm -f "$pid_file"
}

stop_port_listener() {
  local port="$1"
  local name="$2"

  if ! command -v ss >/dev/null 2>&1; then
    return 0
  fi

  local pids
  pids="$(ss -ltnp "sport = :$port" 2>/dev/null | sed -n 's/.*pid=\([0-9]\+\).*/\1/p' | sort -u)"
  if [[ -z "$pids" ]]; then
    return 0
  fi

  echo "Stopping process listening on $name port $port: $pids"
  while IFS= read -r pid; do
    [[ -n "$pid" ]] || continue
    kill "$pid" >/dev/null 2>&1 || true
  done <<<"$pids"

  sleep 1

  pids="$(ss -ltnp "sport = :$port" 2>/dev/null | sed -n 's/.*pid=\([0-9]\+\).*/\1/p' | sort -u)"
  while IFS= read -r pid; do
    [[ -n "$pid" ]] || continue
    kill -9 "$pid" >/dev/null 2>&1 || true
  done <<<"$pids"
}

stop_all() {
  stop_pid_file "$PID_DIR/frontend.pid" "frontend"
  stop_pid_file "$PID_DIR/backend.pid" "backend"
  stop_pid_file "$PID_DIR/hardhat.pid" "hardhat"
  stop_port_listener "$FRONTEND_PORT" "frontend"
  stop_port_listener "$BACKEND_PORT" "backend"
  stop_port_listener "$HARDHAT_PORT" "hardhat"

  if command -v docker >/dev/null 2>&1 && docker compose version >/dev/null 2>&1; then
    docker compose -f "$ROOT_DIR/infra/docker-compose.yml" stop postgres >/dev/null 2>&1 || true
  fi
}

open_firewall_ports() {
  if [[ "${OPEN_FIREWALL:-true}" != "true" ]] || ! command -v ufw >/dev/null 2>&1; then
    return 0
  fi

  local sudo_cmd=()
  if [[ "${EUID:-$(id -u)}" -ne 0 ]]; then
    if sudo -n true >/dev/null 2>&1; then
      sudo_cmd=(sudo)
    else
      echo "ufw exists, but sudo without password is not available. Open ports $FRONTEND_PORT and $BACKEND_PORT manually if needed."
      return 0
    fi
  fi

  "${sudo_cmd[@]}" ufw allow "$FRONTEND_PORT/tcp" >/dev/null 2>&1 || true
  "${sudo_cmd[@]}" ufw allow "$BACKEND_PORT/tcp" >/dev/null 2>&1 || true
}

print_tail_hint() {
  local name="$1"
  local log_file="$2"
  echo "  $name log: tail -f \"$log_file\""
}

if [[ "${1:-start}" == "stop" ]]; then
  stop_all
  echo "Stopped VDS stand."
  exit 0
fi

require_command docker
require_command npm
require_command node
require_command dotnet
require_command curl
require_command timeout
require_node_version

if ! docker compose version >/dev/null 2>&1; then
  echo "Docker Compose plugin is required."
  exit 1
fi

load_or_create_env
open_firewall_ports

echo "Stopping previous app processes..."
stop_pid_file "$PID_DIR/frontend.pid" "frontend"
stop_pid_file "$PID_DIR/backend.pid" "backend"
stop_pid_file "$PID_DIR/hardhat.pid" "hardhat"
stop_port_listener "$FRONTEND_PORT" "frontend"
stop_port_listener "$BACKEND_PORT" "backend"
stop_port_listener "$HARDHAT_PORT" "hardhat"

echo "Starting PostgreSQL..."
docker compose -f "$ROOT_DIR/infra/docker-compose.yml" up -d postgres

echo "Waiting for PostgreSQL..."
postgres_ready=false
for _ in {1..90}; do
  if docker exec document-verifier-postgres pg_isready -U "$POSTGRES_USER" -d "$POSTGRES_DB" >/dev/null 2>&1; then
    echo "PostgreSQL is ready."
    postgres_ready=true
    break
  fi
  sleep 1
done

if [[ "$postgres_ready" != "true" ]]; then
  echo "PostgreSQL did not become ready."
  echo "Check it with: docker logs document-verifier-postgres"
  exit 1
fi

echo "Installing blockchain dependencies..."
cd "$ROOT_DIR/blockchain"
install_npm_deps

echo "Starting Hardhat node..."
npm run node >"$LOG_DIR/hardhat.log" 2>&1 &
echo $! >"$PID_DIR/hardhat.pid"
wait_for_tcp "127.0.0.1" "$HARDHAT_PORT" "Hardhat RPC"

echo "Deploying smart contract..."
npm run deploy:local >"$LOG_DIR/deploy.log" 2>&1
CONTRACT_ADDRESS="$(node -p "require('./deployments/localhost.json').contractAddress")"
echo "Contract address: $CONTRACT_ADDRESS"

echo "Restoring and building backend..."
cd "$ROOT_DIR/backend"
dotnet restore DocumentVerifier.sln
dotnet build DocumentVerifier.sln --no-restore

echo "Starting backend on 0.0.0.0:$BACKEND_PORT..."
ASPNETCORE_ENVIRONMENT="Development" \
ASPNETCORE_URLS="http://0.0.0.0:$BACKEND_PORT" \
ConnectionStrings__Postgres="Host=localhost;Port=5432;Database=$POSTGRES_DB;Username=$POSTGRES_USER;Password=$POSTGRES_PASSWORD" \
Jwt__SigningKey="$JWT_SIGNING_KEY" \
Cors__AllowAnyOrigin="true" \
Cors__FrontendOrigin="$PUBLIC_FRONTEND_URL" \
Https__Redirect="false" \
Blockchain__UseMock="false" \
Blockchain__RpcUrl="http://127.0.0.1:$HARDHAT_PORT" \
Blockchain__ChainId="31337" \
Blockchain__ContractAddress="$CONTRACT_ADDRESS" \
Blockchain__PrivateKey="$HARDHAT_PRIVATE_KEY" \
Blockchain__NetworkName="hardhat-local" \
dotnet run --project src/DocumentVerifier.Api --no-build >"$LOG_DIR/backend.log" 2>&1 &
echo $! >"$PID_DIR/backend.pid"
wait_for_url "http://127.0.0.1:$BACKEND_PORT/api/health" "Backend" "$LOG_DIR/backend.log" "$PID_DIR/backend.pid"
check_cors_preflight

echo "Installing frontend dependencies..."
cd "$ROOT_DIR/frontend/web"
install_npm_deps

echo "Starting frontend on 0.0.0.0:$FRONTEND_PORT..."
if [[ "$FRONTEND_MODE" == "prod" ]]; then
  if ! BACKEND_INTERNAL_URL="http://127.0.0.1:$BACKEND_PORT" NEXT_PUBLIC_API_URL="" npm run build >"$LOG_DIR/frontend-build.log" 2>&1; then
    echo "Frontend build failed."
    echo
    echo "Last frontend build log lines:"
    tail -n 120 "$LOG_DIR/frontend-build.log"
    exit 1
  fi
  BACKEND_INTERNAL_URL="http://127.0.0.1:$BACKEND_PORT" NEXT_PUBLIC_API_URL="" npm run start -- --hostname 0.0.0.0 --port "$FRONTEND_PORT" >"$LOG_DIR/frontend.log" 2>&1 &
else
  BACKEND_INTERNAL_URL="http://127.0.0.1:$BACKEND_PORT" NEXT_PUBLIC_API_URL="" npm run dev -- --hostname 0.0.0.0 --port "$FRONTEND_PORT" >"$LOG_DIR/frontend.log" 2>&1 &
fi
echo $! >"$PID_DIR/frontend.pid"
wait_for_url "http://127.0.0.1:$FRONTEND_PORT" "Frontend" "$LOG_DIR/frontend.log" "$PID_DIR/frontend.pid"
check_frontend_api_proxy

cat <<EOF

VDS stand is ready.

Open from anywhere:
  Frontend: $PUBLIC_FRONTEND_URL
  Backend:  $PUBLIC_BACKEND_URL
  Swagger:  $PUBLIC_BACKEND_URL/swagger

Demo users:
  admin@example.local    / admin123
  issuer@example.local   / issuer123
  verifier@example.local / verifier123

Logs:
$(print_tail_hint "frontend" "$LOG_DIR/frontend.log")
$(print_tail_hint "backend" "$LOG_DIR/backend.log")
$(print_tail_hint "hardhat" "$LOG_DIR/hardhat.log")
  deploy log: tail -f "$LOG_DIR/deploy.log"

Stop:
  ./scripts/start-vds.sh stop

Override public host:
  PUBLIC_HOST=your.domain.example ./scripts/start-vds.sh
EOF
