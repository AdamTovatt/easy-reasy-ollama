using Message = EasyReasy.Ollama.Common.Message;

namespace EasyReasy.Ollama.Server.Extensions
{
    /// <summary>
    /// Extension methods for converting between Message types.
    /// </summary>
    public static class MessageExtensions
    {
        /// <summary>
        /// Converts a <see cref="EasyReasy.Ollama.Common.Message"/> to an <see cref="OllamaSharp.Models.Chat.Message"/>.
        /// </summary>
        /// <param name="message">The message to convert.</param>
        /// <returns>The converted OllamaSharp message.</returns>
        public static OllamaSharp.Models.Chat.Message ToOllamaSharp(this Message message)
        {
            return new OllamaSharp.Models.Chat.Message
            {
                Role = message.Role?.ToOllamaSharp() ?? OllamaSharp.Models.Chat.ChatRole.User,
                Content = message.Content,
                Images = message.Images
            };
        }

        /// <summary>
        /// Converts an <see cref="OllamaSharp.Models.Chat.Message"/> to a <see cref="EasyReasy.Ollama.Common.Message"/>.
        /// </summary>
        /// <param name="message">The OllamaSharp message to convert.</param>
        /// <returns>The converted message.</returns>
        public static Message ToCommon(this OllamaSharp.Models.Chat.Message message)
        {
            return new Message
            {
                Role = message.Role?.ToCommon(),
                Content = message.Content,
                Images = message.Images
            };
        }

        /// <summary>
        /// Converts a collection of <see cref="EasyReasy.Ollama.Common.Message"/> to a collection of <see cref="OllamaSharp.Models.Chat.Message"/>.
        /// </summary>
        /// <param name="messages">The messages to convert.</param>
        /// <returns>The converted OllamaSharp messages.</returns>
        public static IEnumerable<OllamaSharp.Models.Chat.Message> ToOllamaSharp(this IEnumerable<Message> messages)
        {
            return messages.Select(m => m.ToOllamaSharp());
        }

        /// <summary>
        /// Converts a collection of <see cref="OllamaSharp.Models.Chat.Message"/> to a collection of <see cref="EasyReasy.Ollama.Common.Message"/>.
        /// </summary>
        /// <param name="messages">The OllamaSharp messages to convert.</param>
        /// <returns>The converted messages.</returns>
        public static IEnumerable<Message> ToCommon(this IEnumerable<OllamaSharp.Models.Chat.Message> messages)
        {
            return messages.Select(m => m.ToCommon());
        }

        /// <summary>
        /// Converts a list of <see cref="EasyReasy.Ollama.Common.Message"/> to a list of <see cref="OllamaSharp.Models.Chat.Message"/>.
        /// </summary>
        /// <param name="messages">The messages to convert.</param>
        /// <returns>The converted OllamaSharp messages.</returns>
        public static List<OllamaSharp.Models.Chat.Message> ToOllamaSharp(this List<Message> messages)
        {
            return messages.Select(m => m.ToOllamaSharp()).ToList();
        }

        /// <summary>
        /// Converts a list of <see cref="OllamaSharp.Models.Chat.Message"/> to a list of <see cref="EasyReasy.Ollama.Common.Message"/>.
        /// </summary>
        /// <param name="messages">The OllamaSharp messages to convert.</param>
        /// <returns>The converted messages.</returns>
        public static List<Message> ToCommon(this List<OllamaSharp.Models.Chat.Message> messages)
        {
            return messages.Select(m => m.ToCommon()).ToList();
        }
    }
} 