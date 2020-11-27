using CommandLine;
using Spectre.Console;

namespace BuildingManager.CommandOptions
{
    [Verb("link-create", HelpText = "Create or update Digital Twin relationship.")]
    public class RelationShipCreateOptions : BaseOptions
    {
        [Option('s', "source",
            HelpText = "The source of the relationship")]
        public string Source { get; set; }
        
        [Option('t', "target",
            HelpText = "The target of the relationship")]
        public string Target { get; set; }

        public override bool Validate()
        {
            AnsiConsole.MarkupLine($"Will create link from {Source} to {Target}");
            return !string.IsNullOrEmpty(Source) && !string.IsNullOrEmpty(Target);
        }
    }
}