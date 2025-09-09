#!/bin/bash
# ===========================================
# Script de Deploy LevelUpClone.Api
# Autor: Rodrigo Luiz Madeira Furlaneti
# ===========================================

# CONFIG
API_PROJECT="./src/LevelUpClone.Api/LevelUpClone.Api.csproj"
PUBLISH_DIR="./publish-api"
SERVER_USER="root"
SERVER_HOST="api.furlaneti.com"
SERVER_PATH="/var/www/levelupclone-api"
SERVICE_NAME="levelupclone-api"

# ===========================================
# 1. Build local (Windows/Linux)
# ===========================================
echo "[1/4] Publicando aplicação..."
dotnet publish "$API_PROJECT" -c Release -r linux-x64 --self-contained false -o "$PUBLISH_DIR"
if [ $? -ne 0 ]; then
  echo "❌ Erro no build."
  exit 1
fi

# ===========================================
# 2. Upload para o servidor
# ===========================================
echo "[2/4] Enviando arquivos para servidor..."
scp -r "$PUBLISH_DIR"/* $SERVER_USER@$SERVER_HOST:$SERVER_PATH/
if [ $? -ne 0 ]; then
  echo "❌ Erro no upload (scp)."
  exit 1
fi

# ===========================================
# 3. Restart service no servidor
# ===========================================
echo "[3/4] Reiniciando serviço no servidor..."
ssh $SERVER_USER@$SERVER_HOST << EOF
  sudo chown -R www-data:www-data $SERVER_PATH
  sudo systemctl daemon-reload
  sudo systemctl restart $SERVICE_NAME
  sudo systemctl enable $SERVICE_NAME
EOF

# ===========================================
# 4. Healthcheck
# ===========================================
echo "[4/4] Testando endpoints..."
ssh $SERVER_USER@$SERVER_HOST << EOF
  echo "--- STATUS ---"
  sudo systemctl status $SERVICE_NAME --no-pager -l | head -n 20

  echo "--- HEALTH ---"
  curl -sS https://$SERVER_HOST/health/ready || echo "Falha no healthcheck"

  echo "--- SWAGGER ---"
  curl -sS -o /dev/null -w "%{http_code}\n" https://$SERVER_HOST/swagger/index.html
EOF

echo "✅ Deploy concluído!"
