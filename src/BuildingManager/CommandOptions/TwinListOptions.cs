using BuildingManager.Configuration;
using CommandLine;

namespace BuildingManager.CommandOptions
{
    [Verb("twin-list", HelpText = "List Digital Twin instances.")]
    public class TwinListOptions : BaseOptions
    {
        public override bool Validate()
        {
            return true;
        }

        [Option('q', "query", HelpText = "The Query to select the digital twins")]
        public string Query { get; set; } = AdtConfig.DefaultQuery;
    }
}