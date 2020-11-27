using System.IO;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Identity.Client;
using Spectre.Console;

namespace BuildingManager.CommandOptions
{
    [Verb("model-deploy", HelpText = "Deploy model to the registry.")]
    public class ModelDeployOptions : BaseOptions
    {
        [Option('f', "filename",
            HelpText = "The filename containing the model information.")]
        public string FileName { get; set; }

        public override bool Validate()
        {
            if (string.IsNullOrEmpty(FileName))
            {
                AnsiConsole.MarkupLine("[red]There was no file name provided in the options[/]");
                return false;
            }

            if (!File.Exists(FileName))
            {
                AnsiConsole.MarkupLine($"[red]The file [green]{FileName}[/] does not exist[/]");
                return false;
            }

            return true;
        }
    }
}