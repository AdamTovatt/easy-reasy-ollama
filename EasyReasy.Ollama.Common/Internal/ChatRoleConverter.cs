using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyReasy.Ollama.Common.Internal
{
    /// <summary>
    /// JSON converter for <see cref="ChatRole"/> to handle serialization and deserialization.
    /// Internal use only.
    /// </summary>
    internal class ChatRoleConverter : JsonConverter<ChatRole>
    {
        /// <summary>
        /// Reads and converts the JSON to type <see cref="ChatRole"/>.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="typeToConvert">The type to convert.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        /// <returns>The converted value.</returns>
        public override ChatRole Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                string? value = reader.GetString();
                return new ChatRole(value);
            }

            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }

        /// <summary>
        /// Writes a specified value as JSON.
        /// </summary>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="value">The value to convert to JSON.</param>
        /// <param name="options">An object that specifies serialization options to use.</param>
        public override void Write(Utf8JsonWriter writer, ChatRole value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}