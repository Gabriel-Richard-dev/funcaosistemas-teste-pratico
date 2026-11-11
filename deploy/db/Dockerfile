# syntax=docker/dockerfile:1
#
# SQL Server 2022 (Linux) com o banco da aplicacao.
# No primeiro start, o BancoDeDados.mdf versionado em App_Data e anexado (FOR ATTACH);
# em todos os starts os scripts de estrutura e procedures (idempotentes) sao reaplicados.
# Contexto de build: raiz do repositorio.
FROM mcr.microsoft.com/mssql/server:2022-latest

USER root
COPY src/FI.WebAtividadeEntrevista/App_Data/BancoDeDados.mdf src/FI.WebAtividadeEntrevista/App_Data/BancoDeDados_log.ldf /seed/
COPY src/FI.AtividadeEntrevista/DAL/Scripts/ /scripts/01_estrutura/
COPY src/FI.AtividadeEntrevista/DAL/Clientes/Procedures/ /scripts/02_procedures/
COPY src/FI.AtividadeEntrevista/DAL/Beneficiarios/Procedures/ /scripts/02_procedures/
COPY deploy/db/app-user.sql /usr/local/share/app-user.sql
COPY deploy/db/entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod 755 /usr/local/bin/entrypoint.sh && chown -R mssql /seed /scripts

USER mssql
HEALTHCHECK --interval=10s --timeout=5s --start-period=60s --retries=12 CMD test -f /tmp/db-ready
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
