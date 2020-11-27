using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Azure;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using BuildingManager.CommandOptions;
using BuildingManager.Configuration;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace BuildingManager
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            try
            {
                AnsiConsole.Render(
                    new FigletText("Azure Digital Twins")
                        .LeftAligned()
                        .Color(Color.Green));
                
                // Load configuration
                var configDirectory = Directory.GetParent(AppContext.BaseDirectory).FullName;
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(configDirectory)
                    .AddJsonFile("appsettings.json", true)
                    .AddJsonFile("appsettings.dev.json", false)
                    .Build();

                var adtConfiguration = new AdtConfig();
                configuration.GetSection("adt").Bind(adtConfiguration);

                var adtInstanceUrl = adtConfiguration.Endpoint;
                var credential = new DefaultAzureCredential();
                var client = new DigitalTwinsClient(new Uri(adtInstanceUrl), credential);

                var parsedResult = await Parser.Default
                    .ParseArguments<ModelDeployOptions, ModelListOptions, TwinCreateOptions, TwinListOptions,
                        RelationShipCreateOptions, RelationShipListOptions>(args)
                    .MapResult(
                        async (ModelDeployOptions opts) => await DeployModel(client, opts),
                        async (ModelListOptions opts) => await ListModels(client, opts),
                        async (TwinCreateOptions opts) => await CreateTwin(client, opts),
                        async (TwinListOptions opts) => await ListTwins(client, opts),
                        async (RelationShipCreateOptions opts) => await CreateRelationship(client, opts),
                        async (RelationShipListOptions opts) => await ListRelationships(client, opts),
                        async (errs) => 1
                    );

                Console.WriteLine("Command done");
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
        }

        private static async Task<int> ListRelationships(DigitalTwinsClient client, RelationShipListOptions opts)
        {
            await ExecuteAdtActionAsync(async () =>
            {
                // Create a table
                var table = new Table();

                // Add some columns
                table.AddColumn(new TableColumn("Source").RightAligned());
                table.AddColumn(new TableColumn("Link").Centered());
                table.AddColumn(new TableColumn("Target").LeftAligned());


                var twinIds = new List<string>();
                if (!string.IsNullOrEmpty(opts.TwinId))
                {
                    twinIds.Add(opts.TwinId);
                }
                else
                {
                    var twins = client.Query<BasicDigitalTwin>(AdtConfig.DefaultQuery);
                    twinIds.AddRange(
                        from twinPage in twins.AsPages()
                        from digitalTwin in twinPage.Values
                        select digitalTwin.Id);
                }

                foreach (var twinId in twinIds)
                {
                    var results = client.GetRelationshipsAsync<BasicRelationship>(twinId);
                    await foreach (var rel in results)
                    {
                        table.AddRow(rel.SourceId, rel.Name, rel.TargetId);
                    }
                }

                table.Alignment = Justify.Center;
                AnsiConsole.Render(table);
            });

            return 0;
        }


        private static async Task<int> CreateRelationship(DigitalTwinsClient client, RelationShipCreateOptions opts)
        {
            if (opts.Validate())
            {
                await ExecuteAdtActionAsync(async () =>
                {
                    var relationship = new BasicRelationship
                    {
                        TargetId = opts.Target,
                        Name = "contains"
                    };

                    var relId = $"{opts.Source}-contains->{opts.Target}";
                    await client.CreateOrReplaceRelationshipAsync(opts.Source, relId, relationship);
                    Console.WriteLine("Created relationship successfully");
                });
            }

            return 0;
        }

        private static async Task<int> ListTwins(DigitalTwinsClient client, TwinListOptions opts)
        {
            if (opts.Validate())
            {
                await ExecuteAdtActionAsync(async () =>
                {

                    var twins = client.Query<BasicDigitalTwin>(opts.Query);
                    foreach (var twinPage in twins.AsPages())
                    {
                        foreach (var digitalTwin in twinPage.Values)
                        {
                            AnsiConsole.MarkupLine($"Twin {digitalTwin.Id}");
                        }
                    }
                    

                });
            }

            return 1;
        }

        private static async Task<int> CreateTwin(DigitalTwinsClient client, TwinCreateOptions opts)
        {
            if (opts.Validate())
            {
                await ExecuteAdtActionAsync(async () =>
                {
                    var twinData = new BasicDigitalTwin
                    {
                        Id = opts.TwinId,
                        Metadata =
                        {
                            ModelId = opts.ModelId
                        }
                    };
                    //twinData.Contents.Add("Created", DateTime.UtcNow.ToString());
                    //twinData.Contents.Add("BlaBla", "Hello world");

                    var response = await client.CreateOrReplaceDigitalTwinAsync(twinData.Id, twinData);
                    AnsiConsole.MarkupLine($"Created twin: {response.Value.Id}");
                });
                return 0;
            }

            return -1;
        }

        private static async Task<int> ListModels(DigitalTwinsClient client, ModelListOptions opts)
        {
            await ExecuteAdtActionAsync(async () =>
            {
                var modelDataList = client.GetModelsAsync();
                AnsiConsole.MarkupLine("[green]Model list[/]");
                await foreach (var md in modelDataList)
                {
                    AnsiConsole.MarkupLine($"{md.Id}");
                }
            });
            return 1;
        }

        private static async Task<int> DeployModel(DigitalTwinsClient client, ModelDeployOptions option)
        {
            if (option.Validate())
            {
                await ExecuteAdtActionAsync(async () =>
                {
                    var dtdl = File.ReadAllTextAsync(option.FileName).Result;
                    var response = client.CreateModelsAsync(new[] {dtdl}).Result;
                    AnsiConsole.MarkupLine(
                        $"Model succesfully created with id [underline yellow]{response.Value.First().Id}[/]");
                });
            }

            return 1;
        }

        private static async Task ExecuteAdtActionAsync(Func<Task> func)
        {
            AnsiConsole.Render(new Rule("[red]Connecting to Azure Digital Twins[/]"));
            try
            {
                await func();
            }
            catch (AggregateException aggEx)
            {
                if (aggEx.InnerException != null)
                {
                    if (aggEx.InnerException is RequestFailedException rex)
                    {
                        AnsiConsole.WriteLine($"{rex.Status}: {rex.Message}");
                        AnsiConsole.WriteException(rex);
                    }
                }

                throw;
            }
            catch (RequestFailedException e)
            {
                AnsiConsole.WriteLine($"{e.Status}: {e.Message}");
                AnsiConsole.WriteException(e);
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
            AnsiConsole.Render(new Rule("[red]Done[/]"));
        }
    }
}