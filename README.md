# EasyReasy.Ollama

A comprehensive .NET ecosystem for building Ollama-based applications with secure authentication, streaming chat completions, and text embeddings. Built on top of [OllamaSharp](https://github.com/awaescher/OllamaSharp).

## Projects

### [EasyReasy.Ollama.Server](./EasyReasy.Ollama.Server/README.md)
A secure .NET API server that provides a RESTful interface to Ollama, with built-in JWT authentication, multi-tenant support, and a web frontend.

### [EasyReasy.Ollama.Common](./EasyReasy.Ollama.Common/README.md)
[![NuGet](https://img.shields.io/badge/nuget-EasyReasy.Ollama.Common-blue.svg)](https://www.nuget.org/packages/EasyReasy.Ollama.Common)

A shared library containing common models, types, and utilities for the EasyReasy.Ollama ecosystem.

### [EasyReasy.Ollama.Client](./EasyReasy.Ollama.Client/README.md)
[![NuGet](https://img.shields.io/badge/nuget-EasyReasy.Ollama.Client-blue.svg)](https://www.nuget.org/packages/EasyReasy.Ollama.Client)

A .NET client library for interacting with the EasyReasy.Ollama.Server API.

## Quick Start
> (Also see quick start sections on the individual projects)
### Server Setup
```bash
# Set environment variables
OLLAMA_URL="http://localhost:11434"
JWT_SIGNING_SECRET="your-super-secret-jwt-signing-key-here"
ALLOWED_MODEL_01="llama3.1"
TENANT_INFO_01="tenant1,api-key-1"

# Run the server
cd EasyReasy.Ollama.Server
dotnet run
```

### Client Usage
```csharp
using EasyReasy.Ollama.Client;

HttpClient httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("https://your-ollama-server.com");

OllamaClient client = await OllamaClient.CreateAuthorizedAsync(httpClient, "your-api-key");

await foreach (ChatResponsePart part in client.Chat.StreamChatAsync("llama3.1", "Hello"))
{
    Console.Write(part.Message);
}
```

## Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Client App    │    │  Ollama Server   │    │  EasyReasy     │
│                 │◄──►│                  │◄──►│  Ollama Server │
│  .NET Client    │    │  (Local/Remote)  │    │  (API Gateway) │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                                              │
         │                                              │
         └─────────────── Shared Models ────────────────┘
                    (EasyReasy.Ollama.Common)
```

## Getting Started

1. **Set up the server**: See [Server README](./EasyReasy.Ollama.Server/README.md) for detailed setup instructions
2. **Use the client**: See [Client README](./EasyReasy.Ollama.Client/README.md) for client library usage
3. **Explore models**: See [Common README](./EasyReasy.Ollama.Common/README.md) for shared data structures

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

MIT