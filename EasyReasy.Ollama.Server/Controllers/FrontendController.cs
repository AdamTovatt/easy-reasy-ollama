using EasyReasy.Ollama.Common;
using EasyReasy.Ollama.Server.Services.Ollama;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyReasy.Ollama.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class FrontendController : ControllerBase
    {
        private readonly IOllamaServiceFactory _ollamaServiceFactory;
        private readonly ResourceManager _resourceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrontendController"/> class.
        /// </summary>
        public FrontendController(IOllamaServiceFactory ollamaServiceFactory, ResourceManager resourceManager)
        {
            _ollamaServiceFactory = ollamaServiceFactory;
            _resourceManager = resourceManager;
        }

        /// <summary>
        /// Serves the main frontend HTML page.
        /// </summary>
        /// <returns>The HTML content for the frontend.</returns>
        [HttpGet]
        public async Task<IActionResult> GetFrontend()
        {
            string htmlContent = await _resourceManager.ReadAsStringAsync(Resources.Frontend.Index);
            return Content(htmlContent, "text/html");
        }

        /// <summary>
        /// Serves the CSS stylesheet for the frontend.
        /// </summary>
        /// <returns>The CSS content for the frontend.</returns>
        [HttpGet("styles.css")]
        public async Task<IActionResult> GetStyles()
        {
            string cssContent = await _resourceManager.ReadAsStringAsync(Resources.Frontend.Styles);
            return Content(cssContent, "text/css");
        }

        /// <summary>
        /// Serves the JavaScript file for the frontend.
        /// </summary>
        /// <returns>The JavaScript content for the frontend.</returns>
        [HttpGet("script.js")]
        public async Task<IActionResult> GetScript()
        {
            string jsContent = await _resourceManager.ReadAsStringAsync(Resources.Frontend.Script);
            return Content(jsContent, "application/javascript");
        }

        /// <summary>
        /// Streams chat responses using Server-Sent Events (SSE).
        /// This endpoint is kept for backward compatibility with the frontend.
        /// </summary>
        /// <param name="text">The user input text.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        [Authorize]
        [HttpGet("stream-sse")]
        public async Task StreamSse(string text, string model, CancellationToken cancellationToken)
        {
            try
            {
                Response.ContentType = "text/event-stream";

                IOllamaChatService chatService = _ollamaServiceFactory.GetChatService(model);
                await foreach (ChatResponsePart responsePart in chatService.GetResponseAsync(text, cancellationToken))
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