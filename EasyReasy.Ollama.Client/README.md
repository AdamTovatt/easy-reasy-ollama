# EasyReasy.Ollama.Client

[![NuGet](https://img.shields.io/badge/nuget-EasyReasy.Ollama.Client-blue.svg)](https://www.nuget.org/packages/EasyReasy.Ollama.Client)

A .NET client library for interacting with the EasyReasy.Ollama.Server API. This library provides a type-safe, authenticated interface for chat completions, embeddings, and model management.

## Overview

EasyReasy.Ollama.Client provides a comprehensive .NET client for the EasyReasy.Ollama.Server API, featuring:

- **🔐 Automatic Authentication**: JWT token management with API key authentication and proactive token validation with retry on expiration
- **💬 Chat Completions**: Streaming chat responses with support for tools and functions
- **📊 Embeddings**: Text embedding generation
- **🤖 Model Management**: Check available models and model availability
- **🔄 Streaming Support**: Real-time streaming responses using Server-Sent Events
- **🛡️ Error Handling**: Comprehensive error handling with exception recreation
- **🔄 Authentication Retry**: Proactive token validation with retry on 401 responses

## Features

- **Authentication**: Automatic JWT token management with API key authentication and proactive token validation with retry logic
- **Chat Streaming**: Real-time streaming chat completions with SSE
- **Function Calling**: Support for Ollama tool/function definitions and calls
- **Embeddings**: Text embedding generation with any supported model
- **Model Discovery**: Check available models and model availability
- **Type Safety**: Full type safety with the Common library models
- **Error Handling**: Serializable exception responses with recreation
- **Cancellation Support**: Full cancellation token support throughout
- **Authentication Retry**: Proactive token validation with retry on 401 responses

## Quick Start

### Installation

```bash
dotnet add package EasyReasy.Ollama.Client
```

### Basic Usage

```csharp
using EasyReasy.Ollama.Client;
using EasyReasy.Ollama.Common;

// Create an HTTP client
HttpClient httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("https://your-ollama-server.com");

// Create an authenticated client
OllamaClient client = await OllamaClient.CreateAuthorizedAsync(httpClient, "your-api-key");

// Stream a chat response
await foreach (ChatResponsePart part in client.Chat.StreamChatAsync("llama3.1", "Hello, how are you?"))
{
    if (part.Message != null)
    {
        Console.Write(part.Message);
    }
    
    if (part.ToolCall != null)
    {
        Console.WriteLine($"Tool call: {part.ToolCall}");
    }
}
```

## Core Components

### `OllamaClient`

The main client class that handles authentication and provides access to specialized clients.

#### Factory Methods

```csharp
// Create an authenticated client (recommended)
OllamaClient client = await OllamaClient.CreateAuthorizedAsync(httpClient, "api-key");

// Create an unauthenticated client (you must authorize before use)
OllamaClient client = OllamaClient.CreateUnauthorized(httpClient, "api-key");
```

#### Model Management

```csharp
// Get all available models
List<string> models = await client.GetAvailableModelsAsync();

// Check if a specific model is available
bool isAvailable = await client.IsModelAvailableAsync("llama3.1");
```

### `IChatClient`

Handles all chat completion operations with streaming support.

#### Simple Text Chat

```csharp
// Stream response for a single text input
await foreach (ChatResponsePart part in client.Chat.StreamChatAsync("llama3.1", "Hello"))
{
    if (part.Message != null)
    {
        Console.Write(part.Message);
    }
}
```

#### Multi-Message Chat

```csharp
List<Message> messages = new List<Message>
{
    new Message(ChatRole.System, "You are a helpful assistant."),
    new Message(ChatRole.User, "What's the weather like?"),
    new Message(ChatRole.Assistant, "I don't have access to real-time weather data."),
    new Message(ChatRole.User, "Can you help me find out?")
};

await foreach (ChatResponsePart part in client.Chat.StreamChatAsync("llama3.1", messages))
{
    if (part.Message != null)
    {
        Console.Write(part.Message);
    }
}
```

#### Function Calling

```csharp
List<ToolDefinition> tools = new List<ToolDefinition>
{
    new ToolDefinition("get_weather", "Get weather information", parameters)
};

List<Message> messages = new List<Message>
{
    new Message(ChatRole.User, "What's the weather in New York?")
};

await foreach (ChatResponsePart part in client.Chat.StreamChatAsync("llama3.1", messages, tools))
{
    if (part.Message != null)
    {
        Console.Write(part.Message);
    }
    
    if (part.ToolCall != null)
    {
        // Handle the tool call
        Function function = part.ToolCall.Function;
        string result = await ExecuteWeatherFunction(function.Arguments);
        
        // Add the result back to the conversation
        messages.Add(new Message(ChatRole.Tool, function.Name, result));
    }
}
```

#### Complete Request Object

```csharp
ChatRequest request = new ChatRequest(
    modelName: "llama3.1",
    messages: new List<Message> { new Message(ChatRole.User, "Hello") },
    toolDefinitions: new List<ToolDefinition> { /* optional tools */ }
);

await foreach (ChatResponsePart part in client.Chat.StreamChatAsync(request))
{
    if (part.Message != null)
    {
        Console.Write(part.Message);
    }
}
```

### `IEmbeddingClient`

Handles text embedding generation.

#### Simple Embedding

```csharp
// Get embeddings for text
EmbeddingResponse response = await client.Embeddings.GetEmbeddingsAsync("llama3.1", "Hello, world!");

// Access the embedding vector
float[] embeddings = response.Embeddings;
```

#### Using Request Object

```csharp
EmbeddingRequest request = new EmbeddingRequest("llama3.1", "Hello, world!");
EmbeddingResponse response = await client.Embeddings.GetEmbeddingsAsync(request);
```

## Advanced Usage

### Error Handling

The client provides comprehensive error handling:

```csharp
try
{
    await foreach (ChatResponsePart part in client.Chat.StreamChatAsync("llama3.1", "Hello"))
    {
        Console.Write(part.Message);
    }
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("Authentication failed. Check your API key.");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Request failed: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

### Cancellation Support

All operations support cancellation:

```csharp
CancellationTokenSource cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromSeconds(30)); // Cancel after 30 seconds

try
{
    await foreach (ChatResponsePart part in client.Chat.StreamChatAsync("llama3.1", "Hello", cts.Token))
    {
        Console.Write(part.Message);
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("Operation was cancelled.");
}
```

### Custom HTTP Client Configuration

```csharp
HttpClient httpClient = new HttpClient();
httpClient.BaseAddress = new Uri("https://your-ollama-server.com");
httpClient.Timeout = TimeSpan.FromMinutes(5);

// Add custom headers if needed
httpClient.DefaultRequestHeaders.Add("User-Agent", "MyApp/1.0");

OllamaClient client = await OllamaClient.CreateAuthorizedAsync(httpClient, "api-key");
```

### Tool Call Handling

Complete example of handling function calls:

```csharp
async Task<string> ExecuteWeatherFunction(string arguments)
{
    // Parse the arguments and call your weather API
    WeatherData weather = await GetWeatherFromAPI(arguments);
    return JsonSerializer.Serialize(weather);
}

async Task HandleChatWithTools()
{
    List<ToolDefinition> tools = new List<ToolDefinition>
    {
        new ToolDefinition("get_weather", "Get weather information", parameters)
    };

    List<Message> messages = new List<Message>
    {
        new Message(ChatRole.User, "What's the weather like in London?")
    };

    await foreach (ChatResponsePart part in client.Chat.StreamChatAsync("llama3.1", messages, tools))
    {
        if (part.Message != null)
        {
            Console.Write(part.Message);
        }
        
        if (part.ToolCall != null)
        {
            Function function = part.ToolCall.Function;
            string result = await ExecuteWeatherFunction(function.Arguments);
            
            // Add the tool result to continue the conversation
            messages.Add(new Message(function.Name, result));
        }
    }
}
```

## Best Practices

### 1. Use Authenticated Client Creation

```csharp
// Good - automatically handles authentication
OllamaClient client = await OllamaClient.CreateAuthorizedAsync(httpClient, "api-key");

// Avoid - requires manual authorization
OllamaClient client = OllamaClient.CreateUnauthorized(httpClient, "api-key");
// You would need to call EnsureAuthorizedForSpecializedClientsAsync() before use
```

### 2. Handle Streaming Responses Properly

```csharp
// Good - handle both message content and tool calls
await foreach (ChatResponsePart part in client.Chat.StreamChatAsync("llama3.1", "Hello"))
{
    if (part.Message != null)
    {
        Console.Write(part.Message);
    }
    
    if (part.ToolCall != null)
    {
        // Handle tool call
        HandleToolCall(part.ToolCall);
    }
}
```

### 3. Use Cancellation Tokens

```csharp
// Good - support cancellation
CancellationTokenSource cts = new CancellationTokenSource();
cts.CancelAfter(TimeSpan.FromMinutes(2));

await foreach (ChatResponsePart part in client.Chat.StreamChatAsync("llama3.1", "Hello", cts.Token))
{
    // Process response
}
```

### 4. Proper Resource Disposal

```csharp
// Good - dispose the client when done
using (OllamaClient client = await OllamaClient.CreateAuthorizedAsync(httpClient, "api-key"))
{
    // Use the client
}
```

### 5. Error Handling

```csharp
try
{
    await foreach (ChatResponsePart part in client.Chat.StreamChatAsync("llama3.1", "Hello"))
    {
        Console.Write(part.Message);
    }
}
catch (UnauthorizedAccessException)
{
    // Handle authentication errors (note: 401 errors are automatically retried once)
}
catch (HttpRequestException ex)
{
    // Handle network/API errors
}
catch (Exception ex)
{
    // Handle unexpected errors
}
```

### 6. Automatic Retry

The client proactively validates tokens before each request and automatically handles token expiration by retrying requests once with a fresh token when validation fails. This means you don't need to implement your own retry logic for 401 responses.

## Integration

This client library works seamlessly with:

- **EasyReasy.Ollama.Server**: The API server this client connects to
- **EasyReasy.Ollama.Common**: Shared models and types
- **EasyReasy.Auth**: Authentication models and utilities

## Error Handling

The client provides comprehensive error handling:

- **Authentication Errors**: `UnauthorizedAccessException` for invalid API keys (with proactive token validation and retry on expiration)
- **Network Errors**: `HttpRequestException` for connection issues
- **Model Errors**: Clear error messages for unsupported models
- **Serialization Errors**: Graceful handling of malformed responses
- **Exception Recreation**: Server exceptions are recreated on the client

## Performance

- **Streaming**: Efficient streaming responses without buffering
- **Connection Reuse**: HTTP client connection pooling
- **Memory Efficient**: Minimal object allocations during streaming
- **Cancellation**: Full cancellation support for long-running operations

## Security

- **JWT Authentication**: Automatic JWT token management
- **API Key Security**: Secure API key handling
- **HTTPS Support**: Full HTTPS support for secure communication
- **Token Refresh**: Proactive token validation with automatic refresh and retry logic for unexpected 401 responses

---

For more details, see the XML documentation in the code or explore the source.