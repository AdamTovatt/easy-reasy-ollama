# EasyReasy.Ollama.Common

[![NuGet](https://img.shields.io/badge/nuget-EasyReasy.Ollama.Common-blue.svg)](https://www.nuget.org/packages/EasyReasy.Ollama.Common)

A shared library containing common models, types, and utilities for the EasyReasy.Ollama ecosystem. This library provides the foundational data structures used by both the client and server components.

## Overview

EasyReasy.Ollama.Common serves as the shared foundation for the EasyReasy.Ollama project, providing:

- **Chat Models**: Request/response structures for chat completions
- **Embedding Models**: Request/response structures for text embeddings
- **Tool/Function Models**: Support for Ollama function calling
- **Error Handling**: Standardized exception responses
- **JSON Utilities**: Centralized serialization settings

## Features

- **🔤 Chat Completions**: Complete models for chat requests and streaming responses
- **📊 Embeddings**: Models for text embedding requests and responses
- **🔧 Function Calling**: Support for Ollama tool/function definitions and calls
- **🛡️ Error Handling**: Serializable exception responses for network transmission
- **📝 JSON Utilities**: Centralized serialization with consistent settings
- **🎭 Chat Roles**: Type-safe chat role enumeration (system, user, assistant, tool)

## Quick Start

### Installation

```bash
dotnet add package EasyReasy.Ollama.Common
```

### Basic Usage

```csharp
using EasyReasy.Ollama.Common;

// Create a chat request
List<Message> messages = new List<Message>
{
    new Message(ChatRole.System, "You are a helpful assistant."),
    new Message(ChatRole.User, "Hello, how are you?")
};

ChatRequest request = new ChatRequest("llama3.1", messages);

// Serialize to JSON
string json = request.ToJson();

// Deserialize from JSON
ChatRequest deserializedRequest = ChatRequest.FromJson(json);
```

## Core Models

### Chat Completions

#### `ChatRequest`
Represents a chat completion request with model, messages, and optional tool definitions.

```csharp
ChatRequest request = new ChatRequest(
    modelName: "llama3.1",
    messages: new List<Message> { new Message(ChatRole.User, "Hello") },
    toolDefinitions: new List<ToolDefinition> { /* optional tools */ }
);
```

#### `ChatResponsePart`
Represents a streaming response part that can contain either message content or tool calls.

```csharp
// Message content
ChatResponsePart messagePart = new ChatResponsePart("Hello, how can I help you?");

// Tool call
ToolCall toolCall = new ToolCall(new Function("get_weather", "{\"location\": \"New York\"}"));
ChatResponsePart toolPart = new ChatResponsePart(toolCall);
```

#### `Message`
Represents a single message in a chat conversation with support for different roles and multimodal content.

```csharp
// Basic message
Message userMessage = new Message(ChatRole.User, "What's the weather like?");

// Message with images (for multimodal models)
Message imageMessage = new Message(ChatRole.User, "Describe this image", new[] { "base64-image-data" });

// Tool message
Message toolMessage = new Message("get_weather", "The weather is sunny and 75°F");
```

#### `ChatRole`
Type-safe enumeration of chat roles with JSON serialization support.

```csharp
ChatRole.System    // System instructions
ChatRole.User      // User input
ChatRole.Assistant // Assistant responses
ChatRole.Tool      // Tool results
```

### Embeddings

#### `EmbeddingRequest`
Request model for text embeddings.

```csharp
EmbeddingRequest request = new EmbeddingRequest("llama3.1", "Hello, world!");
```

#### `EmbeddingResponse`
Response model containing the embedding vector.

```csharp
EmbeddingResponse response = new EmbeddingResponse(new float[] { 0.1f, 0.2f, 0.3f, ... });
```

### Function Calling

#### `ToolDefinition`
Defines a function that can be called by the model.

```csharp
ToolDefinition tool = new ToolDefinition(
    name: "get_weather",
    description: "Get the current weather for a location",
    parameters: new List<PossibleParameter> { /* parameter definitions */ }
);
```

#### `ToolCall`
Represents a function call made by the model.

```csharp
ToolCall toolCall = new ToolCall(new Function("get_weather", "{\"location\": \"New York\"}"));
```

#### `Function`
Contains the function name and arguments.

```csharp
Function function = new Function("get_weather", "{\"location\": \"New York\"}");
```

### Error Handling

#### `ExceptionResponse`
Serializable exception response for network transmission.

```csharp
try
{
    // Some operation that might throw
}
catch (Exception ex)
{
    ExceptionResponse errorResponse = new ExceptionResponse(ex.GetType(), ex.Message);
    string json = errorResponse.ToJson();
    
    // Later, recreate the exception
    Exception recreated = errorResponse.RecreateException();
}
```

## JSON Serialization

### Centralized Settings

The library provides centralized JSON serialization settings through `JsonSerializerSettings.CurrentOptions`:

```csharp
// Use the centralized settings
string json = JsonSerializer.Serialize(obj, JsonSerializerSettings.CurrentOptions);

// Or set custom options
JsonSerializerSettings.CurrentOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};
```

### Built-in Serialization

All models include built-in JSON serialization methods:

```csharp
// Serialize
string json = request.ToJson();

// Deserialize
ChatRequest request = ChatRequest.FromJson(json);
```

## Advanced Features

### Multimodal Support

Support for image-based models like LLaVA:

```csharp
Message message = new Message(
    ChatRole.User, 
    "What's in this image?", 
    new[] { "base64-encoded-image-data" }
);
```

### Tool/Function Calling

Complete function calling support:

```csharp
// Define available tools
List<ToolDefinition> tools = new List<ToolDefinition>
{
    new ToolDefinition("get_weather", "Get weather information", parameters),
    new ToolDefinition("get_time", "Get current time", parameters)
};

// Create request with tools
ChatRequest request = new ChatRequest("llama3.1", messages, tools);

// Handle tool calls in responses
foreach (ChatResponsePart part in responseStream)
{
    if (part.ToolCall != null)
    {
        // Execute the tool call
        object result = ExecuteTool(part.ToolCall.Function);
    }
}
```

### Streaming Responses

Models support streaming responses through `ChatResponsePart`:

```csharp
// Each part contains either message content or tool calls
foreach (ChatResponsePart part in responseStream)
{
    if (part.Message != null)
    {
        Console.Write(part.Message); // Stream text
    }
    
    if (part.ToolCall != null)
    {
        // Handle tool call
        HandleToolCall(part.ToolCall);
    }
}
```

## Best Practices

### 1. Use Type-Safe Chat Roles
```csharp
// Good
Message message = new Message(ChatRole.User, "Hello");

// Avoid
Message message = new Message("user", "Hello");
```

### 2. Handle Tool Calls Properly
```csharp
if (responsePart.ToolCall != null)
{
    Function function = responsePart.ToolCall.Function;
    object result = await ExecuteFunction(function.Name, function.Arguments);
}
```

### 3. Use Centralized JSON Settings
```csharp
// Always use the centralized settings for consistency
string json = JsonSerializer.Serialize(obj, JsonSerializerSettings.CurrentOptions);
```

### 4. Handle Exceptions Gracefully
```csharp
try
{
    ChatRequest request = ChatRequest.FromJson(json);
}
catch (ArgumentException ex)
{
    // Handle deserialization errors
    var errorResponse = new ExceptionResponse(ex.GetType(), ex.Message);
}
```

## Integration

This library is designed to work seamlessly with:

- **EasyReasy.Ollama.Server**: Provides the data models for the API server
- **EasyReasy.Ollama.Client**: Provides the data models for the client library
- **EasyReasy.Auth**: Compatible with authentication models

## Error Handling

The library provides comprehensive error handling:

- **Deserialization Errors**: All `FromJson` methods throw `ArgumentException` with clear messages
- **Serialization Errors**: Graceful handling of JSON serialization issues
- **Exception Responses**: Network-safe exception transmission
- **Type Safety**: Compile-time checking for chat roles and model structures

## Performance

- **Efficient Serialization**: Optimized JSON serialization settings
- **Memory Efficient**: Minimal object allocations
- **Type Safety**: Compile-time validation reduces runtime errors
- **Streaming Support**: Designed for efficient streaming responses

---

For more details, see the XML documentation in the code or explore the source.