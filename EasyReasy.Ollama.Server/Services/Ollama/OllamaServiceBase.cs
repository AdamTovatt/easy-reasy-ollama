using OllamaSharp;

namespace EasyReasy.Ollama.Server.Services.Ollama
{
    public abstract class OllamaServiceBase : IDisposable
    {
        protected const string _negativeKeepAliveValue = "-1";
        protected OllamaApiClient _client;
        protected bool _keepModelLoaded;
        private bool _disposed;

        protected OllamaServiceBase(string url, string model, bool keepModelLoaded)
        {
            _client = new OllamaApiClient(url, model);
            _keepModelLoaded = keepModelLoaded;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _client.Dispose();
            _disposed = true;
        }
    }
}