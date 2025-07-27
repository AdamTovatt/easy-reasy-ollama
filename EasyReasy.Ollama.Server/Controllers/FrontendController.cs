using EasyReasy.Ollama.Common;
using EasyReasy.Ollama.Server.Services.Ollama;
using Microsoft.AspNetCore.Mvc;

namespace EasyReasy.Ollama.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class FrontendController : ControllerBase
    {
        private readonly IOllamaChatService _ollamaChatService;
        private readonly ResourceManager _resourceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrontendController"/> class.
        /// </summary>
        public FrontendController(IOllamaChatService ollamaChatService, ResourceManager resourceManager)
        {
            _ollamaChatService = ollamaChatService;
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
        /// </summary>
        /// <param name="text">The user input text.</param>
        /// <param name="cancellationToken">Cancellation token for the request.</param>
        [HttpGet("stream-sse")]
        public async Task StreamSse(string text, CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";
            await foreach (ChatResponsePart responsePart in _ollamaChatService.GetResponseAsync(text, cancellationToken))
            {
                await Response.WriteAsync($"data: {responsePart}\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);
            }
        }
    }
}