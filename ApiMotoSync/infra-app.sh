#!/usr/bin/env bash

set -euo pipefail

ORACLE_HOST="${1:-}"
ORACLE_PORT="${2:-}"
ORACLE_SID="${3:-}"
ORACLE_USER="${4:-}"
ORACLE_PASS="${5:-}"

log() {
  echo "[infra-app] $1"
}

fail_if_empty() {
  local value="$1"
  local message="$2"
  if [[ -z "$value" ]]; then
    log "ERRO: $message"
    exit 1
  fi
}

fail_if_empty "$ORACLE_HOST" "Informe o host Oracle como primeiro argumento."
fail_if_empty "$ORACLE_PORT" "Informe a porta Oracle como segundo argumento."
fail_if_empty "$ORACLE_SID" "Informe o SID Oracle como terceiro argumento."
fail_if_empty "$ORACLE_USER" "Informe o usuário Oracle como quarto argumento."
fail_if_empty "$ORACLE_PASS" "Informe a senha Oracle como quinto argumento."

log "Validação das variáveis concluída com sucesso."

CONNECTION_STRING="Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=${ORACLE_HOST})(PORT=${ORACLE_PORT}))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=${ORACLE_SID})));User Id=${ORACLE_USER};Password=********;"
log "Connection string prevista: ${CONNECTION_STRING}"

if command -v sqlplus >/dev/null 2>&1; then
  log "sqlplus detectado. Validando versão instalada..."
  if ! sqlplus -V; then
    log "Aviso: sqlplus retornou erro durante verificação de versão."
  fi
else
  log "sqlplus não encontrado. Execute 'sudo apt-get install -y alien libaio1 unixodbc' e instale o Instant Client se desejar validar a conectividade."
fi

if command -v tnsping >/dev/null 2>&1; then
  log "Executando tnsping para ${ORACLE_HOST}:${ORACLE_PORT}/${ORACLE_SID}..."
  if tnsping "${ORACLE_HOST}:${ORACLE_PORT}/${ORACLE_SID}"; then
    log "tnsping finalizado com sucesso."
  else
    log "tnsping falhou. Verifique firewall ou credenciais."
  fi
else
  log "tnsping não encontrado. Pule a verificação ou instale o Oracle Instant Client."
fi

log "Script de infraestrutura finalizado."

