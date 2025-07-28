using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Represents a tool call within a message.
    /// </summary>
    public class ToolCall
    {
        /// <summary>
        /// Gets or sets the function to be called by the tool.
        /// </summary>
        [JsonPropertyName("function")]
        public Function? Function { get; set; }

        /// <summary>
        /// Creates a <see cref="ToolCall"/> instance from a JSON string.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>A <see cref="ToolCall"/> instance.</returns>
        /// <exception cref="ArgumentException">Thrown when the JSON cannot be deserialized into a <see cref="ToolCall"/>.</exception>
        public static ToolCall FromJson(string json)
        {
            try
            {
                ToolCall? result = JsonSerializer.Deserialize<ToolCall>(json, JsonSerializerSettings.CurrentOptions);

                if (result == null)
                {
                    throw new ArgumentException($"Failed to deserialize {nameof(ToolCall)} from json: {json}");
                }

                return result;
            }
            catch (JsonException jsonException)
            {
                throw new ArgumentException($"Failed to deserialize {nameof(ToolCall)} from json: {json}", jsonException);
            }
        }

        /// <summary>
        /// Creates a <see cref="ToolCall"/> instance from a JSON string with optional error handling.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="throwOnError">Whether to throw an exception on deserialization failure. If false, returns null instead.</param>
        /// <returns>A <see cref="ToolCall"/> instance, or null if deserialization fails and <paramref name="throwOnError"/> is false.</returns>
        /// <exception cref="ArgumentException">Thrown when the JSON cannot be deserialized into a <see cref="ToolCall"/> and <paramref name="throwOnError"/> is true.</exception>
        public static ToolCall? FromJson(string json, bool throwOnError)
        {
            try
            {
                ToolCall? result = JsonSerializer.Deserialize<ToolCall>(json, JsonSerializerSettings.CurrentOptions);

                if (result == null && throwOnError)
                {
                    throw new ArgumentException($"Failed to deserialize {nameof(ToolCall)} from json: {json}");
                }

                return result;
            }
            catch (JsonException jsonException)
            {
                if (throwOnError)
                {
                    throw new ArgumentException($"Failed to deserialize {nameof(ToolCall)} from json: {json}", jsonException);
                }

                return null;
            }
        }

        /// <summary>
        /// Serializes this <see cref="ToolCall"/> instance to a JSON string.
        /// </summary>
        /// <returns>A JSON string representation of this <see cref="ToolCall"/> instance.</returns>
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, JsonSerializerSettings.CurrentOptions);
        }

        /// <summary>
        /// Returns a JSON string representation of this <see cref="ToolCall"/> instance.
        /// </summary>
        /// <returns>A JSON string representation of this <see cref="ToolCall"/> instance.</returns>
        public override string ToString()
        {
            return ToJson();
        }
    }
}