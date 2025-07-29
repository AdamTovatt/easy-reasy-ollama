using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Represents a part of a response from an LLM model, which can contain either a message or a tool call.
    /// </summary>
    public class ChatResponsePart
    {
        /// <summary>
        /// Gets or sets the message content of the response part.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Gets or sets the tool call in the response part.
        /// </summary>
        public ToolCall? ToolCall { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatResponsePart"/> class with both message and tool call.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="toolCall">The tool call.</param>
        [JsonConstructor]
        public ChatResponsePart(string? message, ToolCall? toolCall)
        {
            Message = message;
            ToolCall = toolCall;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatResponsePart"/> class with only a message.
        /// Tool call is assumed to be null.
        /// </summary>
        /// <param name="message">The message content.</param>
        public ChatResponsePart(string message)
        {
            Message = message;
            ToolCall = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatResponsePart"/> class with only a tool call.
        /// Message is assumed to be null.
        /// </summary>
        /// <param name="toolCall">The tool call.</param>
        public ChatResponsePart(ToolCall toolCall)
        {
            Message = null;
            ToolCall = toolCall;
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

        /// <summary>
        /// Overrides the ToString method so that it returns the json version of the object.
        /// </summary>
        public override string ToString()
        {
            return ToJson();
        }
    }
}
