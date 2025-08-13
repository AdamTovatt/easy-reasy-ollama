# EasyReasy.Ollama.Server

A secure .NET API server that provides a RESTful interface to Ollama, with built-in JWT authentication, multi-tenant support, and a web frontend.

## Features

- **🔐 Secure Authentication**: JWT-based authentication with API key support
- **🏢 Multi-tenant**: Support for multiple tenants with isolated API keys
- **🤖 Ollama Integration**: RESTful API for chat completions and embeddings
- **🌐 Web Frontend**: Built-in HTML/JS frontend for easy testing
- **📊 Server-Sent Events**: Real-time streaming chat responses
- **🛡️ Security**: Progressive delay protection against brute-force attacks
- **⚙️ Configurable**: Environment-based configuration for models and tenants

## Quick Start

> **Note**: This quick start is for the **server**. If you're looking for how to use the client library, see the [EasyReasy.Ollama.Client README](../EasyReasy.Ollama.Client/README.md).

## Installation via script
Run this script to install the server:
```bash
wget -qO /tmp/install.sh https://raw.githubusercontent.com/AdamTovatt/easy-reasy-ollama/refs/heads/master/EasyReasy.Ollama.Server/install_script_.sh && sudo bash /tmp/install.sh
```

You can now configure the service:
```bash
sudo nano /etc/systemd/system/easy-reasy-ollama.service
```

### 1. Environment Setup

Set the required environment variables:

```bash
# Ollama connection
OLLAMA_URL="http://localhost:11434"

# JWT signing secret (at least 32 characters)
JWT_SIGNING_SECRET="your-super-secret-jwt-signing-key-here"

# Allowed models (at least one required)
# Can be named anything as long as they start with ALLOWED_MODEL
ALLOWED_MODEL_01="llama3.1"
ALLOWED_MODEL_02="llama3.2"
ALLOWED_MODEL_DEV="llama3.1:latest"

# Tenant configuration (at least one required)
# Format: tenant-id,tenant-api-key (comma-separated)
TENANT_INFO_01="tenant1,api-key-1"
TENANT_INFO_02="tenant2,api-key-2"
TENANT_INFO_DEV="dev-tenant,dev-api-key"
```

### 2. Run the Server

```bash
dotnet run
```

The server will start on `https://localhost:5001` (or the configured port).

### 3. Access the Web Frontend

Open your browser to `https://localhost:5001` to access the built-in web interface for testing chat functionality.

## API Endpoints

### Authentication

#### POST `/api/auth/apikey`
Authenticate with an API key to obtain a JWT token.

**Request:**
```json
{
  "apiKey": "your-api-key-here"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-06-10T12:34:56Z"
}
```

### Chat Completions

#### POST `/api/chat/stream`
Stream chat completions using Server-Sent Events (SSE).

**Headers:**
```
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

**Request:**
```json
{
  "modelName": "llama3.1",
  "messages": [
    {
      "role": "user",
      "content": "Hello, how are you?"
    }
  ],
  "toolDefinitions": [
    {
      "name": "get_weather",
      "description": "Get the current weather",
      "parameters": []
    }
  ]
}
```

**Response:** Server-Sent Events stream with chat response parts. The stream ends when the response is complete.

**Example SSE Response:**
```
data: {"message": "Hello"}

data: {"message": "! How"}

data: {"message": " can"}

data: {"message": " I"}

data: {"message": " help"}

data: {"message": " you"}

data: {"message": " today"}

data: {"message": "?"}
```

Each `data:` line contains a JSON object with:
- `message`: The text content for this part of the response (optional)
- `toolCall`: The tool call information (optional)

**Tool Call Example:**
```
data: {"toolCall": {"name": "get_weather", "arguments": {"location": "New York"}}}
```

**Note:** Tool calls are sent as a single SSE event message, not broken into multiple streaming parts.

### Embeddings

#### POST `/api/embeddings`
Get embeddings for text using the specified model.

**Headers:**
```
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

**Request:**
```json
{
  "modelName": "llama3.1",
  "text": "Hello, world!"
}
```

**Response:**
```json
{
  "embeddings": [0.1, 0.2, 0.3, ...]
}
```

### Frontend

#### GET `/stream-sse`
Endpoint for the built-in web frontend. Requires authentication.

**Query Parameters:**
- `text`: The input text
- `model`: The model name

**Response:** Server-Sent Events stream.

## Configuration

### Environment Variables

| Variable | Required | Description | Example |
|----------|----------|-------------|---------|
| `OLLAMA_URL` | Yes | Ollama server URL | `http://localhost:11434` |
| `JWT_SIGNING_SECRET` | Yes | JWT signing secret (min 32 chars) | `your-secret-key-here` |
| `ALLOWED_MODEL_*` | Yes | Allowed Ollama models | `ALLOWED_MODEL_01=llama3.1` |
| `TENANT_INFO_*` | Yes | Tenant API keys | `TENANT_INFO_01=tenant1,key1` |

### Environment Variable Lists

The server automatically reads environment variables as lists when they follow a specific naming pattern:

#### Models (`ALLOWED_MODEL_*`)
The server scans for all environment variables that start with `ALLOWED_MODEL` and treats them as a list of allowed models. You can name them anything as long as they start with the prefix:

```bash
ALLOWED_MODEL_01="llama3.1"
ALLOWED_MODEL_02="llama3.2"
ALLOWED_MODEL_PROD="llama3.1:latest"
ALLOWED_MODEL_DEV="llama3.1:latest"
```

#### Tenants (`TENANT_INFO_*`)
The server scans for all environment variables that start with `TENANT_INFO` and treats them as a list of tenant configurations. Each value should be formatted as `tenant-id,api-key`:

```bash
TENANT_INFO_01="tenant1,api-key-1"
TENANT_INFO_02="tenant2,api-key-2"
TENANT_INFO_DEV="dev-tenant,dev-api-key"
TENANT_INFO_PROD="prod-tenant,prod-api-key"
```

This approach allows you to easily add or remove models and tenants by simply adding or removing environment variables without changing code.

## Security

### Authentication Flow

1. **API Key Authentication**: Clients authenticate with API keys via `/api/auth/apikey`
2. **JWT Token**: Valid API keys receive a JWT token with tenant information
3. **Protected Endpoints**: All API endpoints require the JWT token in the `Authorization` header
4. **Progressive Delay**: Failed authentication attempts are slowed down to prevent brute-force attacks

### Security Features

- **JWT Authentication**: All endpoints require valid JWT tokens
- **Multi-tenant Isolation**: Each tenant has isolated API keys
- **Model Restrictions**: Only configured models are allowed
- **Progressive Delay**: Automatic protection against brute-force attacks
- **HTTPS**: Secure communication (in production)

## Error Handling

### HTTP Status Codes

- `200 OK`: Successful request
- `400 Bad Request`: Invalid request (e.g., unsupported model)
- `401 Unauthorized`: Invalid or missing authentication
- `500 Internal Server Error`: Server error

### Error Response Format

```json
{
  "exceptionType": "System.ArgumentException",
  "message": "Model 'invalid-model' is not allowed"
}
```

## Architecture

### Key Components

- **Controllers**: Handle HTTP requests and responses
- **Services**: Business logic for Ollama integration
- **Providers**: Configuration providers for models and tenants
- **Middleware**: Authentication and security middleware
- **Frontend**: Built-in web interface for testing

### Authentication Implementation

The server uses the [EasyReasy.Auth](../EasyReasy.Auth/README.md) library for:
- JWT token creation and validation
- API key authentication endpoints
- Claims injection middleware
- Progressive delay protection

## Nginx and SSE

When deploying behind nginx proxies, Server-Sent Events (SSE) may not work properly due to nginx buffering. The server automatically adds the `X-Accel-Buffering: no` header to SSE responses, but you must also configure nginx to respect this header.

### Nginx Configuration

For each nginx instance in your proxy chain, add the following configuration to your location block:

```nginx
location /api/chat/stream {
    proxy_pass http://upstream-server;
    proxy_set_header X-Accel-Buffering no;
    proxy_buffering off;
    proxy_cache off;
}
```

### Key Settings

- `proxy_set_header X-Accel-Buffering no`: Tells nginx to disable buffering for this response
- `proxy_buffering off`: Disables response buffering
- `proxy_cache off`: Disables caching for this location

### Reloading Nginx

After making configuration changes, reload nginx:

```bash
sudo nginx -s reload
```

Or restart the service:

```bash
sudo systemctl reload nginx
```

### Testing Configuration

Validate your nginx configuration before reloading:

```bash
sudo nginx -t
```

## Client Libraries

The project includes client libraries for easy integration:

- **EasyReasy.Ollama.Client**: .NET client library
- **EasyReasy.Ollama.Common**: Shared models and types

See the respective README files for usage examples. 