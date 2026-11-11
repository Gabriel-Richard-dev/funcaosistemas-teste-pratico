# Função Sistemas · Teste Prático: CPF e Beneficiários

Implementação do teste prático do processo seletivo da **Função Sistemas** sobre a solution
`FI.WebAtividadeEntrevista` (ASP.NET MVC 5 / .NET Framework 4.8 / SQL Server).

**Demonstração online:** https://funcaosistemas.gabrielrichard.dev

---

## O que foi pedido e onde está

### 1. CPF do cliente

| Requisito | Como foi atendido |
|---|---|
| Novo campo **CPF** na tela de cadastro/alteração, no padrão visual da tela | `Views/Cliente/Forms.cshtml`, ao lado de Nacionalidade, conforme o layout do enunciado |
| Cadastro obrigatório | `required` no HTML + `[Required]` no `ClienteModel` + validação no `BoCliente` |
| Máscara `999.999.999-99` | `Scripts/Clientes/FI.Cpf.js` formata enquanto o usuário digita |
| Validação dos dígitos verificadores | Front-end (`FI.Cpf.js`), atributo `[Cpf]` no model e `Util/Cpf.cs` na camada de negócio |
| CPF duplicado não é permitido | Verificação no `BoCliente` (`FI_SP_VerificaCliente`) **e** constraint `UQ_CLIENTES_CPF` no banco |
| Coluna `CPF` na tabela `CLIENTES` | `DAL/Scripts/01_Estrutura_CPF_Beneficiarios.sql` (já aplicado ao `BancoDeDados.mdf`) |

### 2. Beneficiários

| Requisito | Como foi atendido |
|---|---|
| Botão **Beneficiários** abrindo um pop-up com CPF e Nome | Modal Bootstrap em `Forms.cshtml` |
| Grid com os beneficiários incluídos, com **Alterar** e **Excluir** | `Scripts/Clientes/FI.Beneficiarios.js` |
| Máscara e validação de CPF | Mesmo utilitário `FI.Cpf.js` no front e `Util/Cpf.cs` no back |
| Mesmo CPF não pode se repetir para o mesmo cliente | Validado no pop-up, no `BoCliente` e pela constraint `UQ_BENEFICIARIOS_CLIENTE_CPF (IDCLIENTE, CPF)` |
| Gravação somente ao clicar em **Salvar** na tela do cliente | Os beneficiários ficam em memória no pop-up e vão no mesmo POST do cliente |
| Tabela `BENEFICIARIOS` com `ID`, `CPF`, `NOME`, `IDCLIENTE` | Script de estrutura + FK `FK_BENEFICIARIOS_CLIENTES` |

---

## Decisões de implementação

- **CPF gravado só com dígitos** (`VARCHAR(11)`). A máscara fica na apresentação, então
  `529.982.247-25` e `52998224725` são tratados como o mesmo CPF na verificação de duplicidade.
- **Validação em três camadas.** O front-end dá retorno imediato, o model (`[Required]`/`[Cpf]`) barra
  requisições inválidas e a camada de negócio (`BoCliente`) é a fonte da verdade, sem depender da tela.
- **Constraints únicas no banco** como última barreira contra duplicidade em condição de corrida.
  Quando a constraint é violada, o erro SQL (2601/2627) vira uma mensagem amigável.
- **Gravação atômica.** Cliente e beneficiários são gravados na mesma transação (`DAL/Padrao/FI.Transacao.cs`).
  Se um beneficiário falhar, o cliente também não é gravado.
- **Sincronização na alteração.** Ao salvar uma alteração, o `BoCliente` compara a lista enviada com a do
  banco: exclui os removidos, altera os mantidos e inclui os novos. As procedures de alteração e exclusão
  filtram por `ID` **e** `IDCLIENTE`, então um id manipulado na requisição não afeta beneficiário de outro cliente.
- **Exclusão de cliente** remove antes os beneficiários (`FI_SP_DelCliente`, em transação), respeitando a FK.
- **Erros de regra de negócio** usam a `RegraNegocioException` e voltam para a tela com HTTP 400.
  O padrão de exibição de erro que já existia nos scripts (`ModalDialog`) foi mantido.
- O JavaScript foi mantido em **ES5** porque o minificador do `System.Web.Optimization` não entende sintaxe ES6+.

## Estrutura do banco

```sql
ALTER TABLE CLIENTES ADD CPF VARCHAR(11) NOT NULL;            -- + UNIQUE (CPF)

CREATE TABLE BENEFICIARIOS (
    ID        BIGINT IDENTITY PRIMARY KEY,
    CPF       VARCHAR(11) NOT NULL,
    NOME      VARCHAR(50) NOT NULL,
    IDCLIENTE BIGINT NOT NULL REFERENCES CLIENTES (ID)        -- + UNIQUE (IDCLIENTE, CPF)
);
```

O `App_Data/BancoDeDados.mdf` versionado **já está com a estrutura e as procedures aplicadas**. Os scripts
ficam versionados em `src/FI.AtividadeEntrevista/DAL` e são idempotentes, então rodar de novo não causa erro:

```
DAL/Scripts/01_Estrutura_CPF_Beneficiarios.sql   colunas, tabela e constraints
DAL/Clientes/Procedures/*.sql                    procedures de cliente (CREATE OR ALTER)
DAL/Beneficiarios/Procedures/*.sql               procedures de beneficiário (CREATE OR ALTER)
```

---

## Como executar

### Opção A: Visual Studio 2022 (como no enunciado)

1. Abra `src/FI.WebAtividadeEntrevista.sln`.
2. Defina `FI.WebAtividadeEntrevista` como projeto de inicialização e pressione **F5**.
   Os pacotes NuGet são restaurados automaticamente. O banco é o LocalDB em `App_Data`, sem passos extras.

### Opção B: Docker (Linux, macOS ou Windows)

```bash
cp .env.example .env        # ajuste as senhas
docker compose up -d --build
```

A aplicação sobe em http://localhost:8080. Detalhes da execução no Linux (Mono + SQL Server 2022) em
[`docs/DEPLOY.md`](docs/DEPLOY.md).

## Testes

O projeto `FI.AtividadeEntrevista.Tests` (MSTest) roda pelo **Test Explorer** do Visual Studio:

- **Unitários**: algoritmo de CPF (`Util/CpfTests.cs`) e regras de validação do `BoCliente`,
  que não precisam de banco.
- **Integração**: inclusão, alteração, duplicidade, sincronização de beneficiários, rollback da transação e
  exclusão. Os testes rodam sobre uma **cópia** do `BancoDeDados.mdf` anexada com outro nome
  (`FI_AtividadeEntrevista_Testes`), então o banco da aplicação não é alterado. Sem o LocalDB instalado, esses
  testes aparecem como *Inconclusivo* em vez de falhar.

## Estrutura do repositório

```
.
├── src/
│   ├── FI.WebAtividadeEntrevista.sln
│   ├── FI.WebAtividadeEntrevista/      # MVC: controllers, models, views, scripts, App_Data (banco)
│   ├── FI.AtividadeEntrevista/         # BLL, DAL (+ scripts SQL), DML, Util (CPF)
│   └── FI.AtividadeEntrevista.Tests/   # testes unitários e de integração
├── deploy/
│   ├── web/                            # imagem da aplicação (Mono + XSP)
│   └── db/                             # imagem do SQL Server 2022 com o banco e os scripts
├── docker-compose.yml                  # execução local
├── docker-compose.prod.yml             # override de produção (rede do proxy reverso)
└── docs/DEPLOY.md                      # hospedagem e detalhes de infraestrutura
```
