using ChatRole = EasyReasy.Ollama.Common.ChatRole;

namespace EasyReasy.Ollama.Server.Extensions
{
    /// <summary>
    /// Extension methods for converting between ChatRole types.
    /// </summary>
    public static class ChatRoleExtensions
    {
        /// <summary>
        /// Converts a <see cref="EasyReasy.Ollama.Common.ChatRole"/> to an <see cref="OllamaSharp.Models.Chat.ChatRole"/>.
        /// </summary>
        /// <param name="role">The chat role to convert.</param>
        /// <returns>The converted OllamaSharp chat role.</returns>
        public static OllamaSharp.Models.Chat.ChatRole ToOllamaSharp(this ChatRole role)
        {
            return role switch
            {
                _ when role.Equals(ChatRole.System) => OllamaSharp.Models.Chat.ChatRole.System,
                _ when role.Equals(ChatRole.Assistant) => OllamaSharp.Models.Chat.ChatRole.Assistant,
                _ when role.Equals(ChatRole.Tool) => OllamaSharp.Models.Chat.ChatRole.Tool,
                _ when role.Equals(ChatRole.User) => OllamaSharp.Models.Chat.ChatRole.User,
                _ => throw new ArgumentException($"Unsupported chat role: {role}", nameof(role))
            };
        }

        /// <summary>
        /// Converts an <see cref="OllamaSharp.Models.Chat.ChatRole"/> to a <see cref="EasyReasy.Ollama.Common.ChatRole"/>.
        /// </summary>
        /// <param name="role">The OllamaSharp chat role to convert.</param>
        /// <returns>The converted chat role.</returns>
        public static ChatRole ToCommon(this OllamaSharp.Models.Chat.ChatRole role)
        {
            return role switch
            {
                _ when role == OllamaSharp.Models.Chat.ChatRole.System => ChatRole.System,
                _ when role == OllamaSharp.Models.Chat.ChatRole.Assistant => ChatRole.Assistant,
                _ when role == OllamaSharp.Models.Chat.ChatRole.Tool => ChatRole.Tool,
                _ => throw new ArgumentException($"Unsupported OllamaSharp chat role: {role}", nameof(role))
            };
        }
    }
}