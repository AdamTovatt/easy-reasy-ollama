using System.Text.Json.Serialization;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Represents a function that can be called by a tool.
    /// </summary>
    public class Function
    {
        /// <summary>
        /// Gets or sets the index of the function.
        /// </summary>
        [JsonPropertyName("index")]
        public int? Index { get; set; }

        /// <summary>
        /// Gets or sets the name of the function.
        /// </summary>
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the arguments for the function, represented as a dictionary of argument names and values.
        /// </summary>
        [JsonPropertyName("arguments")]
        public IDictionary<string, object?>? Arguments { get; set; }
    }
}