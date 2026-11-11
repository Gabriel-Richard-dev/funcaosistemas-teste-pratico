#!/bin/bash
# Inicia o SQL Server, garante que o banco da aplicacao exista e aplica os scripts.
set -euo pipefail

rm -f /tmp/db-ready

DB_NAME="${DB_NAME:-BancoDeDados}"
APP_DB_USER="${APP_DB_USER:-fi_app}"
DATA_DIR=/var/opt/mssql/data
SQLCMD=(/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -b)

/opt/mssql/bin/sqlservr &
pid=$!

echo "[init] Aguardando o SQL Server..."
for _ in $(seq 1 90); do
    "${SQLCMD[@]}" -Q "SELECT 1" >/dev/null 2>&1 && break
    sleep 2
done

existe=$("${SQLCMD[@]}" -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = '$DB_NAME'")
if [ "$existe" = "0" ]; then
    echo "[init] Anexando $DB_NAME a partir do .mdf versionado"
    cp /seed/BancoDeDados.mdf "$DATA_DIR/$DB_NAME.mdf"
    cp /seed/BancoDeDados_log.ldf "$DATA_DIR/${DB_NAME}_log.ldf"
    "${SQLCMD[@]}" -Q "CREATE DATABASE [$DB_NAME] ON (FILENAME = '$DATA_DIR/$DB_NAME.mdf'), (FILENAME = '$DATA_DIR/${DB_NAME}_log.ldf') FOR ATTACH;"
fi

for script in /scripts/01_estrutura/*.sql /scripts/02_procedures/*.sql; do
    echo "[init] Aplicando $(basename "$script")"
    "${SQLCMD[@]}" -d "$DB_NAME" -i "$script"
done

# Usuario de aplicacao com privilegio minimo (leitura, escrita e execucao de procedures)
if [ -n "${APP_DB_PASSWORD:-}" ]; then
    echo "[init] Garantindo o login de aplicacao '$APP_DB_USER'"
    "${SQLCMD[@]}" -d "$DB_NAME" -v AppUser="$APP_DB_USER" AppPassword="$APP_DB_PASSWORD" -i /usr/local/share/app-user.sql
fi

touch /tmp/db-ready
echo "[init] Banco pronto."
wait $pid
