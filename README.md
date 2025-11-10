# API MotoSync

Integrantes:
Vinicius Murtinho Vicente: Rm551151 
Lucas Barreto Consentino: RM557107
Gustavo Bispo Cordeiro: RM558515


API .NET 8 minimalista para gestão de motos, usuários e filiais com integração ao Oracle Database, preparada para implantação via Azure DevOps (classic pipelines) conforme tutorial fornecido.

## Estrutura

- `MotoSync.sln` – solução raiz.
- `ApiMotoSync/` – projeto principal Web API.
  - `Domain/Entities` – modelos de domínio.
  - `Infrastructure/Data` – `DbContext`, configurações e migrações EF Core Oracle.
  - `Controllers` – endpoints REST para `Filiais`, `Usuarios` e `Motos`.
  - `infra-app.sh` – script usado pela tarefa Azure CLI para validar variáveis Oracle.
- `MotoSync.Tests/` – testes com `Microsoft.EntityFrameworkCore.InMemory`.

> Build verificado em $(date +"%d/%m/%Y %H:%M") UTC.

## Status da Pipeline

- Build: ![CI](https://dev.azure.com/motosync/MotoSync%20Devops/_apis/build/status/MotoSync%20Devops-ASP.NET%20Core-CI)
- Release: Pipeline clássica `Deploy em Dev` publica em `api-motosync.azurewebsites.net`.

## Variáveis de ambiente

Configure as variáveis na pipeline conforme tabela abaixo (marque `Settable at queue time`):

| Nome | Valor sugerido | Uso |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Development` | Ambiente Web API |
| `OracleConnection` | `Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=$(ORACLE_HOST))(PORT=$(ORACLE_PORT)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=$(ORACLE_SID))));User Id=$(ORACLE_USER);Password=$(ORACLE_PASS);` | String de conexão consumida pela API |
| `ORACLE_HOST` | `oracle.fiap.com.br` | Host Oracle |
| `ORACLE_PORT` | `1521` | Porta Oracle |
| `ORACLE_SID` | `ORCL` | SID Oracle |
| `ORACLE_USER` | `rm558515` | Usuário Oracle |
| `ORACLE_PASS` | `Fiap#2025` | Senha Oracle |
| `LOCATION` | `brazilsouth` | Região Azure |
| `NOME_WEBAPP` | `api-motosync` | Nome do Web App criado pela automação |
| `RESOURCE_GROUP` | `rg-motosync` | Grupo de recursos padrão |
| `APP_PLAN` | `plan-motosync` | Plano de App Service utilizado |

> A string final é aplicada em `appsettings.json` via `ConnectionStrings:OracleConnection`. Em produção, use Azure App Service Application Settings para sobrepor valores sensíveis.

## Execução local

```bash
dotnet restore
dotnet ef database update --project ApiMotoSync/ApiMotoSync.csproj --startup-project ApiMotoSync/ApiMotoSync.csproj
dotnet run --project ApiMotoSync/ApiMotoSync.csproj
```

- Swagger: `https://localhost:7041/swagger`
- Endpoint de saúde: `GET https://localhost:7041/wellcome`

> Se não tiver acesso ao Oracle, mantenha `Database:RunInitialization = false` (configuração padrão) em `appsettings.json` para evitar tentativas de seed.

## Pipeline (resumo do tutorial)

1. **Projeto Azure DevOps**: `Pricewhisper` (Git, Agile, Private).  
2. **Importação**: repositório Git da aplicação.  
3. **Service Connection**: `MyAzureSubscription` (Azure Resource Manager, Subscription).  
4. **Pipeline CI (Classic)**:
   - Agente inicial `ubuntu-24.04` (job `Criar Infra Inicial`) executa `Azure CLI` com `ApiMotoSync/infra-app.sh $(ORACLE_HOST) $(ORACLE_PORT) $(ORACLE_SID) $(ORACLE_USER) $(ORACLE_PASS)`.
   - Job `Build and Test` (Windows 2025) usa template ASP.NET Core com tarefas `Restore`, `Build`, `Test` (título `Testes de CRUD`), `Publish`, `Publish Artifact` (nome `app`).  
   - Habilite `Only when all previous tasks have succeeded` em todas as tasks e adicione dependência do job `Build and Test` para o job `Criar Infra Inicial`.
5. **Pipeline Release (Classic)**:
   - Template `Azure App Service deployment` com stage `Deploy em Dev`, agente Windows 2025.
   - App Service: `api-dotnet-rm9999` (ajuste para seu nome).
   - App settings: `-ASPNETCORE_ENVIRONMENT $(ASPNETCORE_ENVIRONMENT) -ConnectionStrings_OracleConnection "$(OracleConnection)"`.
   - Artefato: build `Pricewhisper-CI`, continuous deployment habilitado para `master`.

Após qualquer commit (ex.: inclusão da rota `/wellcome`), a pipeline de build e release deve executar automaticamente. Valide o deploy acessando `https://api-dotnet-rm9999.azurewebsites.net/swagger` ou `.../wellcome`.

## Scripts úteis

- `ApiMotoSync/infra-app.sh`: provisiona tabelas Oracle e cria/atualiza recursos Azure (Resource Group, App Service Plan e Web App), além de habilitar logging.
- Domínio padrão após o deploy: `https://api-motosync.azurewebsites.net/`

## Limpeza

Finalize o laboratório removendo recursos criados na assinatura Azure e descartando tabelas Oracle se necessário (`drop table "Usuarios";`, `drop table "Empresas";`). Ajuste os nomes conforme o schema real.

