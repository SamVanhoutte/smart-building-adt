using CommandLine;

namespace BuildingManager.CommandOptions
{
    [Verb("link-list", HelpText = "List all relationships in the registry.")]
    public class RelationShipListOptions : BaseOptions
    {
        public override bool Validate()
        {
            return true;
        }

        [Option('t', "twinid",
            HelpText = "The id of the twin you want to list relations for")]
        public string TwinId { get; set; }
    }
}