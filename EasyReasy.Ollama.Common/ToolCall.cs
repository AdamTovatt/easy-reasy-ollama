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
    }
} 