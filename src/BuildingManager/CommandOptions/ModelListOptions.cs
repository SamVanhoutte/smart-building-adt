using System.Threading.Tasks;
using CommandLine;

namespace BuildingManager.CommandOptions
{
    [Verb("model-list", HelpText = "List all models in the registry.")]
    public class ModelListOptions : BaseOptions
    {
        public override bool Validate()
        {
            return true;
        }
    }
}