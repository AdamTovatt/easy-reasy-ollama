using EasyReasy.Ollama.Common;
using System.Runtime.CompilerServices;
using System.Text;

namespace EasyReasy.Ollama.Client
{
    /// <summary>
    /// Implementation of the chat client that handles all chat-related operations.
    /// </summary>
    public class ChatClient : IChatClient
    {
        private readonly HttpClient _httpClient;
        private readonly OllamaClient _mainClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HTTP client to use.</param>
        /// <param name="mainClient">The main Ollama client for authorization.</param>
        public ChatClient(HttpClient httpClient, OllamaClient mainClient)
        {
            _httpClient = httpClient;
            _mainClient = mainClient;
        }

        /// <summary>
        /// Streams chat responses for a single text input.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="text">The user input text.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        public IAsyncEnumerable<ChatResponsePart> StreamChatAsync(string modelName, string text, CancellationToken cancellationToken = default)
        {
            List<Message> messages = new List<Message>
            {
                new Message(ChatRole.User, text)
            };

            return StreamChatAsync(modelName, messages, cancellationToken);
        }

        /// <summary>
        /// Streams chat responses for a list of messages.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        public IAsyncEnumerable<ChatResponsePart> StreamChatAsync(string modelName, List<Message> messages, CancellationToken cancellationToken = default)
        {
            ChatRequest request = new ChatRequest(modelName, messages);
            return StreamChatAsync(request, cancellationToken);
        }

        /// <summary>
        /// Streams chat responses for a list of messages with tool definitions.
        /// </summary>
        /// <param name="modelName">The name of the model to use.</param>
        /// <param name="messages">The list of messages to send to the model.</param>
        /// <param name="toolDefinitions">The list of tool definitions to include in the request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        public IAsyncEnumerable<ChatResponsePart> StreamChatAsync(string modelName, List<Message> messages, IEnumerable<ToolDefinition> toolDefinitions, CancellationToken cancellationToken = default)
        {
            ChatRequest request = new ChatRequest(modelName, messages, toolDefinitions);
            return StreamChatAsync(request, cancellationToken);
        }

        /// <summary>
        /// Streams chat responses using a complete request object.
        /// </summary>
        /// <param name="request">The request containing model name, messages and optional tool definitions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An async stream of response parts from the model.</returns>
        public async IAsyncEnumerable<ChatResponsePart> StreamChatAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await _mainClient.EnsureAuthorizedForSpecializedClientsAsync(cancellationToken);

            string json = request.ToJson();
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, "api/chat/stream")
            {
                Content = content
            };

            HttpResponseMessage response;
            bool retried = false;

            while (true)
            {
                try
                {
                    response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    // Re-throw TaskCanceledException to preserve cancellation semantics
                    throw;
                }
                catch (HttpRequestException httpEx) when (httpEx.InnerException is TaskCanceledException)
                {
                    // Unwrap TaskCanceledException from HttpRequestException
                    throw httpEx.InnerException;
                }
                catch (HttpRequestException httpEx) when (cancellationToken.IsCancellationRequested)
                {
                    // If cancellation is requested and we get an HttpRequestException, 
                    // it's likely due to cancellation, so throw TaskCanceledException
                    throw new TaskCanceledException("Request was cancelled", httpEx, cancellationToken);
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && !retried)
                {
                    // Token might be expired, force refresh and retry once
                    retried = true;
                    
                    // Dispose the failed response
                    response.Dispose();
                    
                    // Force authorization refresh
                    await _mainClient.ForceAuthorizeAsync(cancellationToken);
                    
                    // Retry the request
                    continue;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && retried)
                {
                    // Already retried once, this is a real authentication failure
                    string errorContent;
                    try
                    {
                        errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        throw;
                    }
                    catch (HttpRequestException httpEx) when (cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException("Request was cancelled", httpEx, cancellationToken);
                    }

                    throw new HttpRequestException($"Authentication failed after retry. Status: {response.StatusCode}, Content: {errorContent}");
                }

                if (!response.IsSuccessStatusCode)
                {
                    string errorContent;
                    try
                    {
                        errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        throw;
                    }
                    catch (HttpRequestException httpEx) when (cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException("Request was cancelled", httpEx, cancellationToken);
                    }

                    if (string.IsNullOrEmpty(errorContent))
                    {
                        errorContent = "(NO CONTENT AVAILABLE)";
                    }

                    string requestUri = response.RequestMessage?.RequestUri?.ToString() ?? "(UNKNOWN REQUEST URI)";

                    throw new HttpRequestException($"Failed to stream chat from {requestUri}. Status: {response.StatusCode}, Content: {errorContent}");
                }

                // Success, break out of retry loop
                break;
            }

            Stream stream;
            try
            {
                stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // Re-throw TaskCanceledException to preserve cancellation semantics
                throw;
            }
            catch (HttpRequestException httpEx) when (httpEx.InnerException is TaskCanceledException)
            {
                // Unwrap TaskCanceledException from HttpRequestException
                throw httpEx.InnerException;
            }
            catch (HttpRequestException httpEx) when (cancellationToken.IsCancellationRequested)
            {
                // If cancellation is requested and we get an HttpRequestException, 
                // it's likely due to cancellation, so throw TaskCanceledException
                throw new TaskCanceledException("Request was cancelled", httpEx, cancellationToken);
            }

            byte[] buffer = new byte[4096];
            StringBuilder lineBuilder = new StringBuilder();

            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0)
                    break; // End of stream

                string chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                for (int i = 0; i < chunk.Length; i++)
                {
                    char c = chunk[i];

                    if (c == '\n')
                    {
                        // Process complete line
                        string line = lineBuilder.ToString().TrimEnd('\r');
                        lineBuilder.Clear();

                        if (line.StartsWith("data: "))
                        {
                            string data = line.Substring(6);

                            if (string.IsNullOrEmpty(data))
                                continue;

                            ChatResponsePart? responsePart = null;
                            Exception? parseException = null;

                            try
                            {
                                responsePart = ChatResponsePart.FromJson(data);
                            }
                            catch (Exception exception)
                            {
                                parseException = exception;
                            }

                            if (responsePart != null)
                            {
                                yield return responsePart;
                            }
                            else if (parseException != null)
                            {
                                // Try to parse as ExceptionResponse
                                try
                                {
                                    ExceptionResponse exceptionResponse = ExceptionResponse.FromJson(data);
                                    Exception recreatedException = exceptionResponse.RecreateException();
                                    throw recreatedException;
                                }
                                catch
                                {
                                    // If it's not an ExceptionResponse, rethrow the original exception
                                    throw parseException;
                                }
                            }
                        }
                    }
                    else
                    {
                        lineBuilder.Append(c);
                    }
                }
            }
        }
    }
}