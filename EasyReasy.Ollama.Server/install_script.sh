#!/bin/bash
set -e

# Variables
ZIP_NAME="linux-arm64.zip"
URL="https://github.com/AdamTovatt/easy-reasy-ollama/releases/download/v1.0.0/$ZIP_NAME"
DEST_DIR="/opt/easy-reasy-ollama"
SERVICE_FILE="/etc/systemd/system/easy-reasy-ollama.service"
ZIP_FILE="/tmp/easy-reasy-ollama.zip"
USER_NAME=$(whoami)

# Download and unzip
curl -L "$URL" -o "$ZIP_FILE"
mkdir -p "$DEST_DIR"
unzip -o "$ZIP_FILE" -d "$DEST_DIR"
chmod +x "$DEST_DIR/EasyReasy.Ollama.Server"

# Create systemd service file
cat <<EOF > "$SERVICE_FILE"
[Unit]
Description=Easy Reasy Ollama Server

[Service]
ExecStart=$DEST_DIR/EasyReasy.Ollama.Server
User=$USER_NAME
Restart=always
RestartSec=4

Environment=OLLAMA_URL=http://localhost:11434
Environment=JWT_SIGNING_SECRET=your-super-secret-jwt-signing-key-here
Environment=ALLOWED_MODEL_01=llama3.1
Environment=ALLOWED_MODEL_02=llama3.2
Environment=ALLOWED_MODEL_DEV=llama3.1:latest
Environment=TENANT_INFO_01=tenant1,api-key-1
Environment=TENANT_INFO_02=tenant2,api-key-2
Environment=TENANT_INFO_DEV=dev-tenant,dev-api-key

[Install]
WantedBy=multi-user.target
EOF

echo ""
echo "Installation completed."
echo "To configure the service, edit:"
echo "   $SERVICE_FILE"
echo "Then run:"
echo "   sudo systemctl daemon-reload"
echo "   sudo systemctl enable --now easy-reasy-ollama.service"
