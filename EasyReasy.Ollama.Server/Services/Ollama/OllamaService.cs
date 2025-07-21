using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;
using System.Runtime.CompilerServices;

namespace EasyReasy.Ollama.Server.Services.Ollama
{
    public class OllamaService : IOllamaService
    {
        private OllamaApiClient _client;
        private bool _keepModelLoaded;

        private OllamaService(string url, string model, bool keepModelLoaded)
        {
            _client = new OllamaApiClient(url, model);
            _keepModelLoaded = keepModelLoaded;
        }

        public static OllamaService Create(string url, string model, bool keepModelLoaded = true)
        {
            return new OllamaService(url, model, keepModelLoaded);
        }

        public async IAsyncEnumerable<string> GetResponseAsync(string text, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            ChatRequest chatRequest = new ChatRequest();

            if (_keepModelLoaded)
                chatRequest.KeepAlive = "-1";

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

        public async Task<IReadOnlyList<float>> GetEmbeddingsAsync(string text, CancellationToken cancellationToken = default)
        {
            EmbedRequest embedRequest = new EmbedRequest
            {
                Model = _client.Model,
                Prompt = text,
            };

            EmbedResponse embedResponse = await _client.EmbedAsync(embedRequest, cancellationToken);

            if (embedResponse == null || embedResponse.Embedding == null)
                return Array.Empty<float>();

            return embedResponse.Embedding;
        }
    }
}
