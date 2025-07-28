using System.Text.Json;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Request model for chat with tools.
    /// </summary>
    public class ChatRequest
    {
        /// <summary>
        /// Gets or sets the name of the model to use.
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Gets or sets the list of messages.
        /// </summary>
        public List<Message> Messages { get; set; }

        /// <summary>
        /// Gets or sets the list of tool definitions.
        /// </summary>
        public IEnumerable<ToolDefinition>? ToolDefinitions { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatRequest"/> class.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="messages">The list of messages.</param>
        /// <param name="toolDefinitions">The optional list of tool definitions.</param>
        public ChatRequest(string modelName, List<Message> messages, IEnumerable<ToolDefinition>? toolDefinitions = null)
        {
            ModelName = modelName;
            Messages = messages;
            ToolDefinitions = toolDefinitions;
        }

        /// <summary>
        /// Serializes this <see cref="ChatRequest"/> instance to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of this <see cref="ChatRequest"/> instance.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, JsonSerializerSettings.CurrentOptions);
        }

        /// <summary>
        /// Returns a JSON string representation of this <see cref="ChatRequest"/> instance.
        /// </summary>
        /// <returns>A JSON string representation of this <see cref="ChatRequest"/> instance.</returns>
        public override string ToString()
        {
            return ToJson();
        }

        /// <summary>
        /// Creates a <see cref="ChatRequest"/> instance from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A <see cref="ChatRequest"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when the JSON cannot be deserialized into a <see cref="ChatRequest"/>.</exception>
        public static ChatRequest FromJson(string json)
        {
            try
            {
                ChatRequest? result = JsonSerializer.Deserialize<ChatRequest>(json, JsonSerializerSettings.CurrentOptions);

                if (result == null)
                {
                    throw new ArgumentException($"Failed to deserialize {nameof(ChatRequest)} from json: {json}");
                }

                return result;
            }
            catch (JsonException jsonException)
            {
                throw new ArgumentException($"Failed to deserialize {nameof(ChatRequest)} from json: {json}", jsonException);
            }
        }
    }
}