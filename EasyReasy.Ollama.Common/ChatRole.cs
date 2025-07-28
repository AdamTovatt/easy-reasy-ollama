using EasyReasy.Ollama.Common.Internal;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace EasyReasy.Ollama.Common
{
    /// <summary>
    /// Represents a role within a chat completions interaction, describing the intended purpose of a message.
    /// </summary>
    [JsonConverter(typeof(ChatRoleConverter))]
    public readonly struct ChatRole : IEquatable<ChatRole>
    {
        private readonly string _value;

        private const string SYSTEM_VALUE = "system";
        private const string ASSISTANT_VALUE = "assistant";
        private const string USER_VALUE = "user";
        private const string TOOL_VALUE = "tool";

        /// <summary>
        /// Gets the role that instructs or sets the behavior of the assistant.
        /// </summary>
        public static ChatRole System { get; } = new ChatRole("system");

        /// <summary>
        /// Gets the role that provides responses to system-instructed, user-prompted input.
        /// </summary>
        public static ChatRole Assistant { get; } = new ChatRole("assistant");

        /// <summary>
        /// Gets the role that provides input for chat completions.
        /// </summary>
        public static ChatRole User { get; } = new ChatRole("user");

        /// <summary>
        /// Gets the role that is used to input the result from an external tool.
        /// </summary>
        public static ChatRole Tool { get; } = new ChatRole("tool");

        /// <summary>
        /// Initializes a new instance of <see cref="ChatRole"/> with the specified role.
        /// </summary>
        /// <param name="role">The role to initialize with.</param>
        /// <exception cref="ArgumentNullException">Thrown when role is null.</exception>
        public ChatRole(string? role)
        {
            _value = role ?? throw new ArgumentNullException(nameof(role));
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ChatRole"/> using a JSON constructor.
        /// </summary>
        /// <param name="_">The placeholder parameter for JSON constructor.</param>
        [JsonConstructor]
        public ChatRole(object _)
        {
            _value = null!;
        }

        /// <summary>
        /// Determines if two <see cref="ChatRole"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="ChatRole"/> to compare.</param>
        /// <param name="right">The second <see cref="ChatRole"/> to compare.</param>
        /// <returns>True if both instances are equal; otherwise, false.</returns>
        public static bool operator ==(ChatRole left, ChatRole right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Determines if two <see cref="ChatRole"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="ChatRole"/> to compare.</param>
        /// <param name="right">The second <see cref="ChatRole"/> to compare.</param>
        /// <returns>True if both instances are not equal; otherwise, false.</returns>
        public static bool operator !=(ChatRole left, ChatRole right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Implicitly converts a string to a <see cref="ChatRole"/>.
        /// </summary>
        /// <param name="value">The string value to convert.</param>
        public static implicit operator ChatRole(string value)
        {
            return new ChatRole(value);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="ChatRole"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current <see cref="ChatRole"/>.</param>
        /// <returns>True if the specified object is equal to the current <see cref="ChatRole"/>; otherwise, false.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
        {
            if (obj is ChatRole other)
            {
                return Equals(other);
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified <see cref="ChatRole"/> is equal to the current <see cref="ChatRole"/>.
        /// </summary>
        /// <param name="other">The <see cref="ChatRole"/> to compare with the current <see cref="ChatRole"/>.</param>
        /// <returns>True if the specified <see cref="ChatRole"/> is equal to the current <see cref="ChatRole"/>; otherwise, false.</returns>
        public bool Equals(ChatRole other)
        {
            return string.Equals(_value, other._value, StringComparison.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current <see cref="ChatRole"/>.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        /// <summary>
        /// Returns a string that represents the current <see cref="ChatRole"/>.
        /// </summary>
        /// <returns>A string that represents the current <see cref="ChatRole"/>.</returns>
        public override string ToString()
        {
            return _value;
        }
    }
}