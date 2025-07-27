using static OllamaSharp.Models.Chat.Message;

namespace EasyReasy.Ollama.Server.Extensions
{
    /// <summary>
    /// Extension methods for converting between Function types.
    /// </summary>
    public static class FunctionExtensions
    {
        /// <summary>
        /// Converts a <see cref="Common.Function"/> to an <see cref="OllamaSharp.Models.Chat.Function"/>.
        /// </summary>
        /// <param name="function">The function to convert.</param>
        /// <returns>The converted OllamaSharp function.</returns>
        public static Function ToOllamaSharp(this Common.Function function)
        {
            return new Function
            {
                Index = function.Index,
                Name = function.Name,
                Arguments = function.Arguments
            };
        }

        /// <summary>
        /// Converts an <see cref="Function"/> to a <see cref="Common.Function"/>.
        /// </summary>
        /// <param name="function">The OllamaSharp function to convert.</param>
        /// <returns>The converted function.</returns>
        public static Common.Function ToCommon(this Function function)
        {
            return new Common.Function
            {
                Index = function.Index,
                Name = function.Name,
                Arguments = function.Arguments
            };
        }

        /// <summary>
        /// Converts a collection of <see cref="Common.Function"/> to a collection of <see cref="Function"/>.
        /// </summary>
        /// <param name="functions">The functions to convert.</param>
        /// <returns>The converted OllamaSharp functions.</returns>
        public static IEnumerable<Function> ToOllamaSharp(this IEnumerable<Common.Function> functions)
        {
            return functions.Select(f => f.ToOllamaSharp());
        }

        /// <summary>
        /// Converts a collection of <see cref="Function"/> to a collection of <see cref="Common.Function"/>.
        /// </summary>
        /// <param name="functions">The OllamaSharp functions to convert.</param>
        /// <returns>The converted functions.</returns>
        public static IEnumerable<Common.Function> ToCommon(this IEnumerable<Function> functions)
        {
            return functions.Select(f => f.ToCommon());
        }

        /// <summary>
        /// Converts a list of <see cref="Common.Function"/> to a list of <see cref="Function"/>.
        /// </summary>
        /// <param name="functions">The functions to convert.</param>
        /// <returns>The converted OllamaSharp functions.</returns>
        public static List<Function> ToOllamaSharp(this List<Common.Function> functions)
        {
            return functions.Select(f => f.ToOllamaSharp()).ToList();
        }

        /// <summary>
        /// Converts a list of <see cref="Function"/> to a list of <see cref="Common.Function"/>.
        /// </summary>
        /// <param name="functions">The OllamaSharp functions to convert.</param>
        /// <returns>The converted functions.</returns>
        public static List<Common.Function> ToCommon(this List<Function> functions)
        {
            return functions.Select(f => f.ToCommon()).ToList();
        }
    }
} 