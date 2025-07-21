namespace EasyReasy.Ollama.Server
{
    /// <summary>
    /// Contains all resource collections for the EasyReasy.Ollama.Server project.
    /// </summary>
    public static class Resources
    {
        /// <summary>
        /// Resource collection for the embedded frontend files.
        /// </summary>
        [ResourceCollection(typeof(EmbeddedResourceProvider))]
        public static class Frontend
        {
            /// <summary>
            /// The main frontend HTML page.
            /// </summary>
            public static readonly Resource Index = new Resource("Resources/Frontend/index.html");

            /// <summary>
            /// The CSS stylesheet for the frontend.
            /// </summary>
            public static readonly Resource Styles = new Resource("Resources/Frontend/styles.css");

            /// <summary>
            /// The JavaScript file for the frontend.
            /// </summary>
            public static readonly Resource Script = new Resource("Resources/Frontend/script.js");
        }
    }
}
