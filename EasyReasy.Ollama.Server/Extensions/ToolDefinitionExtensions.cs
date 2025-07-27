using EasyReasy.Ollama.Common;
using OllamaSharp.Models.Chat;

namespace EasyReasy.Ollama.Server.Extensions
{
    public static class ToolDefinitionExtensions
    {
        public static Tool ToOllamaTool(this ToolDefinition toolDefinition)
        {
            Parameters? parameters = null;

            if (toolDefinition.Parameters != null)
            {
                parameters = new Parameters();

                Dictionary<string, Property> properties = new Dictionary<string, Property>();
                List<string> required = new List<string>();

                foreach (PossibleParameter parameter in toolDefinition.Parameters)
                {
                    properties[parameter.ParameterName] = new Property()
                    {
                        Type = parameter.Type,
                        Description = parameter.Description,
                    };

                    if (parameter.Required)
                        required.Add(parameter.ParameterName);
                }
            }

            return new Tool()
            {
                Function = new OllamaSharp.Models.Chat.Function
                {
                    Description = toolDefinition.Description,
                    Name = toolDefinition.Name,
                    Parameters = parameters,
                },
                Type = toolDefinition.Type,
            };
        }

        public static IEnumerable<Tool> ToOllamaTools(this IEnumerable<ToolDefinition> toolDefinitions)
        {
            List<Tool> tools = new List<Tool>();

            foreach (ToolDefinition toolDefinition in toolDefinitions)
            {
                tools.Add(toolDefinition.ToOllamaTool());
            }

            return tools;
        }
    }
}
