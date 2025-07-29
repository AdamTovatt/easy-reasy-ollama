using EasyReasy.Ollama.Common;
using EasyReasy.Ollama.Server.Services.Ollama;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyReasy.Ollama.Server.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IOllamaServiceFactory _ollamaServiceFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChatController"/> class.
        /// </summary>
        public ChatController(IOllamaServiceFactory ollamaServiceFactory)
        {
            _ollamaServiceFactory = ollamaServiceFactory;
        }

        /// <summary>
        /// Streams chat responses using Server-Sent Events (SSE).
        /// </summary>
        /// <param name="request">The request containing model name, messages and optional tool definitions.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        [HttpPost("stream")]
        public async Task StreamChatResponse([FromBody] ChatRequest request, CancellationToken cancellationToken)
        {
            try
            {
                 Response.ContentType = "text/event-stream";

                IOllamaChatService chatService = _ollamaServiceFactory.GetChatService(request.ModelName);
                await foreach (ChatResponsePart responsePart in chatService.GetResponseAsync(request.Messages, request.ToolDefinitions, cancellationToken))
                {
                    await Response.WriteAsync($"data: {responsePart}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (Exception exception)
            {
                if (exception is ModelNotAllowedException || exception is ArgumentException)
                    Response.StatusCode = 400;
                else
                    Response.StatusCode = 500;

                ExceptionResponse exceptionResponse = new ExceptionResponse(exception.GetType(), exception.Message);
                await Response.WriteAsync($"data: {exceptionResponse.ToJson()}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
    }
}