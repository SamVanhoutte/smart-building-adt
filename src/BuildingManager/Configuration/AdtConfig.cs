namespace BuildingManager.Configuration
{
    public class AdtConfig
    {
        public string Endpoint { get; set; }
        public const string DefaultQuery = "SELECT * FROM digitaltwins";
    }
}