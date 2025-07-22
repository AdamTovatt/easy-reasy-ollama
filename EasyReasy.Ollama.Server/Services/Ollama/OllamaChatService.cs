using OllamaSharp.Models.Chat;
using System.Runtime.CompilerServices;

namespace EasyReasy.Ollama.Server.Services.Ollama
{
    public class OllamaChatService : OllamaServiceBase, IOllamaChatService
    {
        private OllamaChatService(string url, string model, bool keepModelLoaded) : base(url, model, keepModelLoaded) { }

        public static OllamaChatService Create(string url, string model, bool keepModelLoaded = true)
        {
            return new OllamaChatService(url, model, keepModelLoaded);
        }

        public async IAsyncEnumerable<string> GetResponseAsync(string text, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ChatRequest chatRequest = new ChatRequest();

            if (_keepModelLoaded)
                chatRequest.KeepAlive = _negativeKeepAliveValue;

            List<Message> messages = new List<Message>();
            messages.Add(new Message(ChatRole.User, text));
            chatRequest.Messages = messages;

            IAsyncEnumerable<ChatResponseStream?> result = _client.ChatAsync(chatRequest, cancellationToken);

            await foreach (ChatResponseStream? message in result)
            {
                if (message == null)
                    continue;

                yield return message.Message.Content ?? "";
            }
        }
    }
}