using System;
using System.IO;
using System.Threading.Tasks;
using Azure.DigitalTwins.Core;
using Azure.Identity;
using BuildingManager.Configuration;
using Microsoft.Extensions.Configuration;

namespace BuildingManager
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            // Build configuration
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
            Console.WriteLine($"Service client created – ready to go");
        }
    }
}