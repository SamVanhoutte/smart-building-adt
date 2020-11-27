using CommandLine;
using Spectre.Console;

namespace BuildingManager.CommandOptions
{
    [Verb("twin-create", HelpText = "Create or update Digital Twin instance.")]
    public class TwinCreateOptions : BaseOptions
    {
        [Option('m', "model",
            HelpText = "The unique Id of the model")]
        public string ModelId { get; set; }
        
        [Option('i', "id",
            HelpText = "The unique Id of the twin")]
        public string TwinId { get; set; }

        public override bool Validate()
        {
            AnsiConsole.MarkupLine($"Will create twin '{TwinId ?? "<None>"}' of model {ModelId ?? "<None>"}");
            return !string.IsNullOrEmpty(ModelId) && !string.IsNullOrEmpty(TwinId);
        }
    }
}