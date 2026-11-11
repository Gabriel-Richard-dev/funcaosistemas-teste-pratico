# syntax=docker/dockerfile:1
#
# Aplicacao ASP.NET MVC 5 (.NET Framework 4.8) executando no Linux via Mono + XSP.
# Contexto de build: raiz do repositorio.

# ---------- build ----------
FROM mono:6.12 AS build
WORKDIR /src

COPY src/FI.WebAtividadeEntrevista.sln ./
COPY src/FI.WebAtividadeEntrevista/packages.config FI.WebAtividadeEntrevista/
RUN nuget restore FI.WebAtividadeEntrevista/packages.config -PackagesDirectory packages -NonInteractive

COPY src/FI.AtividadeEntrevista/ FI.AtividadeEntrevista/
COPY src/FI.WebAtividadeEntrevista/ FI.WebAtividadeEntrevista/
RUN msbuild FI.WebAtividadeEntrevista/FI.WebAtividadeEntrevista.csproj \
        /p:Configuration=Release /nologo /verbosity:minimal

# Monta somente o que o site precisa em tempo de execucao
RUN set -eu; \
    mkdir -p /site; cd FI.WebAtividadeEntrevista; \
    cp -r bin Content Scripts Views Global.asax Web.config favicon.ico /site/; \
    rm -rf /site/bin/roslyn

# ---------- runtime ----------
FROM mono:6.12
# A imagem oficial e baseada no Debian 10 (buster), cujos repositorios foram movidos para o archive
RUN set -eu; \
    printf '%s\n' 'deb http://archive.debian.org/debian buster main' 'deb http://archive.debian.org/debian-security buster/updates main' > /etc/apt/sources.list; \
    apt-get -o Acquire::Check-Valid-Until=false update; \
    apt-get install -y --no-install-recommends mono-xsp4 xmlstarlet; \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /site/ ./

# Ajustes do Web.config para o ambiente Linux/producao:
#  - remove o compilador Roslyn (csc.exe do Windows); o Mono compila as views com o compilador nativo
#  - desliga o modo debug (habilita bundling/minificacao)
#  - connection string passa a vir da variavel de ambiente CONNECTION_STRING (ver entrypoint.sh)
#  - customErrors em RemoteOnly para nao expor stack traces
RUN set -eu; \
    xmlstarlet ed -L \
        -d '/configuration/system.codedom' \
        -u '/configuration/system.web/compilation/@debug' -v 'false' \
        -u '/configuration/connectionStrings/add[@name="BancoDeDados"]/@connectionString' -v '__CONNECTION_STRING__' \
        -s '/configuration/system.web' -t elem -n customErrors \
        -i '/configuration/system.web/customErrors' -t attr -n mode -v RemoteOnly \
        Web.config; \
    rm -rf App_Data

COPY deploy/web/entrypoint.sh /usr/local/bin/entrypoint.sh
RUN chmod +x /usr/local/bin/entrypoint.sh

EXPOSE 8080
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
CMD ["xsp4", "--port", "8080", "--address", "0.0.0.0", "--root", "/app", "--nonstop", "--verbose"]
