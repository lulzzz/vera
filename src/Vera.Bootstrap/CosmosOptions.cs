namespace Vera.Bootstrap
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

    public class CosmosContainerOptions
    {
        public const string Section = "VERA:COSMOS:CONTAINER";

        /// <summary>
        /// Name of the invoices container.
        /// </summary>
        public string Invoices { get; set; }

        /// <summary>
        /// Name of the companies container.
        /// </summary>
        public string Companies { get; set; }

        /// <summary>
        /// Names of the users container.
        /// </summary>
        public string Users { get; set; }
    }
}