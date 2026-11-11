# Hospedagem

A aplicação original roda em Windows (IIS Express + SQL Server LocalDB). Para publicá-la numa VPS Linux
sem mudar o código da solution, ela foi containerizada assim:

```
             Internet (HTTPS)
                    │
          ┌─────────▼─────────┐
          │   Caddy (proxy)   │  certificado Let's Encrypt automático
          └─────────┬─────────┘
                    │ rede docker "caddy_web"
          ┌─────────▼─────────┐
          │   web (Mono/XSP)  │  ASP.NET MVC 5 / .NET Framework 4.8
          └─────────┬─────────┘
                    │ rede interna do compose
          ┌─────────▼─────────┐
          │ db (SQL Server    │  2022 Express (Linux)
          │     2022)         │  volume persistente
          └───────────────────┘
```

## Imagem `web` (`deploy/web/Dockerfile`)

- **Build** com `mono:6.12`: `nuget restore` + `msbuild /p:Configuration=Release`.
- **Runtime** com Mono + **XSP4** (servidor ASP.NET do Mono) na porta 8080.
- Ajustes aplicados **somente na imagem** (o `Web.config` do repositório continua igual ao do Visual Studio):
  - remove o `<system.codedom>` (Roslyn/`csc.exe` é binário Windows; o Mono compila as views Razor com o próprio compilador);
  - `compilation debug="false"`, que habilita bundling e minificação;
  - `customErrors mode="RemoteOnly"`, para não expor stack trace;
  - a connection string vem da variável `CONNECTION_STRING` (injetada pelo `entrypoint.sh`).

## Imagem `db` (`deploy/db/Dockerfile`)

- Base `mcr.microsoft.com/mssql/server:2022-latest`, edição **Express**.
- No **primeiro start** o `BancoDeDados.mdf` versionado em `App_Data` é anexado (`FOR ATTACH`). Assim o banco
  publicado é exatamente o mesmo entregue na solution.
- **Em todo start** os scripts de `DAL/Scripts` e as procedures são reaplicados (são idempotentes).
- Cria o login `fi_app`, com privilégio mínimo (`db_datareader`, `db_datawriter` e `EXECUTE`), usado pela aplicação.
  O `sa` não é usado pela aplicação.
- `HEALTHCHECK`: o container web só sobe depois que o banco termina a inicialização.

## Executar localmente

```bash
cp .env.example .env
docker compose up -d --build
# http://localhost:8080
```

## Publicar na VPS

Pré-requisitos: Docker, um proxy reverso (aqui o Caddy, em container, na rede `caddy_web`) e um
registro DNS `A` apontando o domínio para a VPS.

```bash
git clone <repo> /opt/funcaosistemas && cd /opt/funcaosistemas
cp .env.example .env    # gere senhas fortes
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

Bloco do Caddyfile:

```caddy
funcaosistemas.gabrielrichard.dev {
    encode gzip
    reverse_proxy funcaosistemas-web-1:8080
}
```

### Atualizar

```bash
git pull
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d --build
```

O volume `db-data` preserva os dados entre atualizações. Para recomeçar do `.mdf` original:
`docker compose down -v` (**apaga os dados cadastrados**).

## Observações

- O Mono implementa o .NET Framework 4.x, e o MVC 5, o Razor, o `System.Web.Optimization` e o `SqlClient` usados pela
  solution funcionam nele sem alteração de código. Em produção Windows o caminho natural seria o IIS, e o código
  da solution é o mesmo nos dois casos.
- Consumo medido: ~110 MB (web) + ~600 MB (SQL Server, limitado por `MSSQL_MEMORY_LIMIT_MB`).
