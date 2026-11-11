#!/bin/sh
# Injeta a connection string (variavel CONNECTION_STRING) no Web.config e inicia o XSP.
set -eu

: "${CONNECTION_STRING:?Defina a variavel de ambiente CONNECTION_STRING}"

# Escapa caracteres especiais para XML e para o sed
valor=$(printf '%s' "$CONNECTION_STRING" | sed -e 's/&/\&amp;/g' -e 's/"/\&quot;/g' -e 's/</\&lt;/g' -e 's/[\/|&\\]/\\&/g')
sed -i "s|__CONNECTION_STRING__|${valor}|" /app/Web.config

exec "$@"
