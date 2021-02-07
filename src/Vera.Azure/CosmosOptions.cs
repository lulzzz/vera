namespace Vera.Azure
{
    public class CosmosOptions
    {
        public const string Section = "VERA:COSMOS";

        /// <summary>
        /// Connection string to the Cosmos instance.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Database where the containers are created.
        /// </summary>
        public string Database { get; set; }
    }
}