using static OllamaSharp.Models.Chat.Message;

namespace EasyReasy.Ollama.Server.Extensions
{
    /// <summary>
    /// Extension methods for converting between ToolCall types.
    /// </summary>
    public static class ToolCallExtensions
    {
        /// <summary>
        /// Converts a <see cref="Common.ToolCall"/> to an <see cref="ToolCall"/>.
        /// </summary>
        /// <param name="toolCall">The tool call to convert.</param>
        /// <returns>The converted OllamaSharp tool call.</returns>
        public static ToolCall ToOllamaSharp(this ToolCall toolCall)
        {
            return new ToolCall
            {
                Function = toolCall.Function?.ToOllamaSharp()
            };
        }

        /// <summary>
        /// Converts an <see cref="ToolCall"/> to a <see cref="Common.ToolCall"/>.
        /// </summary>
        /// <param name="toolCall">The OllamaSharp tool call to convert.</param>
        /// <returns>The converted tool call.</returns>
        public static ToolCall ToCommon(this ToolCall toolCall)
        {
            return new ToolCall
            {
                Function = toolCall.Function?.ToCommon()
            };
        }

        /// <summary>
        /// Converts a collection of <see cref="Common.ToolCall"/> to a collection of <see cref="ToolCall"/>.
        /// </summary>
        /// <param name="toolCalls">The tool calls to convert.</param>
        /// <returns>The converted OllamaSharp tool calls.</returns>
        public static IEnumerable<ToolCall> ToOllamaSharp(this IEnumerable<ToolCall> toolCalls)
        {
            return toolCalls.Select(tc => tc.ToOllamaSharp());
        }

        /// <summary>
        /// Converts a collection of <see cref="ToolCall"/> to a collection of <see cref="Common.ToolCall"/>.
        /// </summary>
        /// <param name="toolCalls">The OllamaSharp tool calls to convert.</param>
        /// <returns>The converted tool calls.</returns>
        public static IEnumerable<ToolCall> ToCommon(this IEnumerable<ToolCall> toolCalls)
        {
            return toolCalls.Select(tc => tc.ToCommon());
        }

        /// <summary>
        /// Converts a list of <see cref="Common.ToolCall"/> to a list of <see cref="ToolCall"/>.
        /// </summary>
        /// <param name="toolCalls">The tool calls to convert.</param>
        /// <returns>The converted OllamaSharp tool calls.</returns>
        public static List<ToolCall> ToOllamaSharp(this List<ToolCall> toolCalls)
        {
            return toolCalls.Select(tc => tc.ToOllamaSharp()).ToList();
        }

        /// <summary>
        /// Converts a list of <see cref="ToolCall"/> to a list of <see cref="Common.ToolCall"/>.
        /// </summary>
        /// <param name="toolCalls">The OllamaSharp tool calls to convert.</param>
        /// <returns>The converted tool calls.</returns>
        public static List<ToolCall> ToCommon(this List<ToolCall> toolCalls)
        {
            return toolCalls.Select(tc => tc.ToCommon()).ToList();
        }
    }
} 