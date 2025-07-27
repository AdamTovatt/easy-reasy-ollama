using System.Text.Json;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Represents a part of a response from an LLM model, which can contain either a message or tool calls.
    /// </summary>
    public class ChatResponsePart
    {
        /// <summary>
        /// Gets or sets the message content of the response part.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the tool calls in the response part.
        /// </summary>
        public List<ToolCall>? ToolCalls { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatResponsePart"/> class with both message and tool calls.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="toolCalls">The tool calls.</param>
        public ChatResponsePart(string? message, List<ToolCall>? toolCalls)
        {
            Message = message;
            ToolCalls = toolCalls;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatResponsePart"/> class with only a message.
        /// Tool calls are assumed to be null.
        /// </summary>
        /// <param name="message">The message content.</param>
        public ChatResponsePart(string message)
        {
            Message = message;
            ToolCalls = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatResponsePart"/> class with only tool calls.
        /// Message is assumed to be null.
        /// </summary>
        /// <param name="toolCalls">The tool calls.</param>
        public ChatResponsePart(List<ToolCall> toolCalls)
        {
            Message = null;
            ToolCalls = toolCalls;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatResponsePart"/> class with only tool calls.
        /// Message is assumed to be null.
        /// </summary>
        /// <param name="toolCalls">The tool calls.</param>
        public ChatResponsePart(IEnumerable<ToolCall> toolCalls)
        {
            Message = null;
            ToolCalls = toolCalls?.ToList();
        }

        /// <summary>
        /// Serializes this <see cref="ChatResponsePart"/> to JSON string.
        /// </summary>
        /// <returns>A JSON string representation of this object.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, options: JsonSerializerSettings.CurrentOptions);
        }

        /// <summary>
        /// Deserializes a JSON string to a <see cref="ChatResponsePart"/> object.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A <see cref="ChatResponsePart"/> object.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the deserialized object is null.</exception>
        public static ChatResponsePart FromJson(string json)
        {
            ChatResponsePart? result = JsonSerializer.Deserialize<ChatResponsePart>(json, JsonSerializerSettings.CurrentOptions);

            if (result == null)
                throw new InvalidOperationException($"Failed to deserialize JSON to ChatResponsePart. The result was null from the json: {json}");

            return result;
        }
    }
}
