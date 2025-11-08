#!/usr/bin/env bash

set -euo pipefail

# Parâmetros vindos da task Azure CLI (posição importa!)
DB_HOST="${1:?Informe o host Oracle}"
DB_PORT="${2:?Informe a porta Oracle}"
DB_SID="${3:?Informe o SID Oracle}"
DB_USER="${4:?Informe o usuário Oracle}"
DB_PASS="${5:?Informe a senha Oracle}"

log() {
  echo "[infra-app] $1"
}

TEMP_DIR="$(pwd)"

# ======================
# Configuração do SQLcl
# ======================

SQLCL_HOME="${HOME}/sqlcl"
SQLCL_BIN="${SQLCL_HOME}/bin/sql"

if [[ ! -x "${SQLCL_BIN}" ]]; then
  log "Baixando SQLcl..."
  curl -sSL -o "${HOME}/sqlcl.zip" "https://download.oracle.com/otn_software/java/sqldeveloper/sqlcl-latest.zip"
  log "Extraindo SQLcl..."
  unzip -q "${HOME}/sqlcl.zip" -d "${HOME}"
fi

# =======================
# Scripts do Banco Oracle
# =======================

log "Gerando script SQL para estruturas do banco..."
cat > "${TEMP_DIR}/cria_objetos.sql" <<'SQL'
WHENEVER SQLERROR EXIT SQL.SQLCODE

BEGIN
  EXECUTE IMMEDIATE q'[
    CREATE TABLE "FILIAIS" (
      "Id" RAW(16) NOT NULL,
      "Nome" NVARCHAR2(160) NOT NULL,
      "Codigo" NVARCHAR2(20) NOT NULL,
      "Endereco" NVARCHAR2(200) NOT NULL,
      "Cidade" NVARCHAR2(120) NOT NULL,
      "Estado" NVARCHAR2(60) NOT NULL,
      CONSTRAINT "PK_FILIAIS" PRIMARY KEY ("Id")
    )
  ]';
EXCEPTION
  WHEN OTHERS THEN IF SQLCODE = -955 THEN NULL; ELSE RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE q'[
    CREATE TABLE "USUARIOS" (
      "Id" RAW(16) NOT NULL,
      "Nome" NVARCHAR2(150) NOT NULL,
      "Email" NVARCHAR2(180) NOT NULL,
      "Cargo" NVARCHAR2(80) NOT NULL,
      "FilialId" RAW(16) NOT NULL,
      CONSTRAINT "PK_USUARIOS" PRIMARY KEY ("Id")
    )
  ]';
EXCEPTION
  WHEN OTHERS THEN IF SQLCODE = -955 THEN NULL; ELSE RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE q'[
    CREATE TABLE "MOTOS" (
      "Id" RAW(16) NOT NULL,
      "Modelo" NVARCHAR2(150) NOT NULL,
      "Placa" NVARCHAR2(10) NOT NULL,
      "Ano" NUMBER(10) NOT NULL,
      "Status" NVARCHAR2(50) DEFAULT 'Disponivel' NOT NULL,
      "FilialId" RAW(16) NOT NULL,
      "GestorId" RAW(16),
      CONSTRAINT "PK_MOTOS" PRIMARY KEY ("Id")
    )
  ]';
EXCEPTION
  WHEN OTHERS THEN IF SQLCODE = -955 THEN NULL; ELSE RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE q'[
    CREATE UNIQUE INDEX "IX_FILIAIS_Codigo" ON "FILIAIS" ("Codigo")
  ]';
EXCEPTION
  WHEN OTHERS THEN IF SQLCODE = -955 THEN NULL; ELSE RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE q'[
    CREATE UNIQUE INDEX "IX_USUARIOS_Email" ON "USUARIOS" ("Email")
  ]';
EXCEPTION
  WHEN OTHERS THEN IF SQLCODE = -955 THEN NULL; ELSE RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE q'[
    CREATE UNIQUE INDEX "IX_MOTOS_Placa" ON "MOTOS" ("Placa")
  ]';
EXCEPTION
  WHEN OTHERS THEN IF SQLCODE = -955 THEN NULL; ELSE RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE q'[
    ALTER TABLE "USUARIOS"
    ADD CONSTRAINT "FK_USUARIOS_FILIAIS_FilialId"
      FOREIGN KEY ("FilialId")
      REFERENCES "FILIAIS" ("Id")
  ]';
EXCEPTION
  WHEN OTHERS THEN IF SQLCODE IN (-2275, -2260) THEN NULL; ELSE RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE q'[
    ALTER TABLE "MOTOS"
    ADD CONSTRAINT "FK_MOTOS_FILIAIS_FilialId"
      FOREIGN KEY ("FilialId")
      REFERENCES "FILIAIS" ("Id")
      ON DELETE CASCADE
  ]';
EXCEPTION
  WHEN OTHERS THEN IF SQLCODE IN (-2275, -2260) THEN NULL; ELSE RAISE; END IF;
END;
/

BEGIN
  EXECUTE IMMEDIATE q'[
    ALTER TABLE "MOTOS"
    ADD CONSTRAINT "FK_MOTOS_USUARIOS_GestorId"
      FOREIGN KEY ("GestorId")
      REFERENCES "USUARIOS" ("Id")
      ON DELETE SET NULL
  ]';
EXCEPTION
  WHEN OTHERS THEN IF SQLCODE IN (-2275, -2260) THEN NULL; ELSE RAISE; END IF;
END;
/

EXIT
SQL

log "Executando script de criação do banco..."
"${SQLCL_BIN}" "${DB_USER}/${DB_PASS}@${DB_HOST}:${DB_PORT}/${DB_SID}" @"${TEMP_DIR}/cria_objetos.sql"
log "[OK] Estruturas do banco garantidas."

# =======================
# Recursos no Microsoft Azure
# =======================

RG_NAME="${RESOURCE_GROUP:-rg-motosync}"
LOCATION="${LOCATION:-brazilsouth}"
PLAN_NAME="${APP_PLAN:-plan-motosync}"
APP_NAME="${NOME_WEBAPP:-api-motosync}"
SKU="${APP_SERVICE_SKU:-B1}"
RUNTIME="${APP_RUNTIME:-DOTNET|8.0}"

log "Criando/atualizando recursos no Azure..."
az group create --name "${RG_NAME}" --location "${LOCATION}" 1>/dev/null

if ! az appservice plan show --name "${PLAN_NAME}" --resource-group "${RG_NAME}" >/dev/null 2>&1; then
  az appservice plan create \
    --name "${PLAN_NAME}" \
    --resource-group "${RG_NAME}" \
    --location "${LOCATION}" \
    --sku "${SKU}" \
    --is-linux 1>/dev/null
fi

if ! az webapp show --resource-group "${RG_NAME}" --name "${APP_NAME}" >/dev/null 2>&1; then
  az webapp create \
    --resource-group "${RG_NAME}" \
    --plan "${PLAN_NAME}" \
    --name "${APP_NAME}" \
    --runtime "${RUNTIME}" >/dev/null
fi

log "Habilitando logs básicos do Web App..."
az webapp log config \
  --resource-group "${RG_NAME}" \
  --name "${APP_NAME}" \
  --application-logging filesystem \
  --detailed-error-messages true \
  --failed-request-tracing true \
  --web-server-logging filesystem \
  --level information 1>/dev/null

log "[OK] Infraestrutura pronta para deploy."
