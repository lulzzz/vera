namespace Vera.Bootstrap
{
    public class CosmosContainerOptions
    {
        public const string Section = "VERA:COSMOS:CONTAINER";

        /// <summary>
        /// Name of the invoices container.
        /// </summary>
        public string Invoices { get; set; } = "invoices";

        /// <summary>
        /// Name of the companies container.
        /// </summary>
        public string Companies { get; set; } = "companies";

        /// <summary>
        /// Names of the users container.
        /// </summary>
        public string Users { get; set; } = "users";

        /// <summary>
        /// Name of the audits container.
        /// </summary>
        public string Audits { get; set; } = "audits";
    }
}