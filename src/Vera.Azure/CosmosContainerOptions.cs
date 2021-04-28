namespace Vera.Azure
{
    public class CosmosContainerOptions
    {
        public const string Section = "VERA:COSMOS:CONTAINER";

        /// <summary>
        /// Name of the invoices container.
        /// </summary>
        public string Invoices { get; init; } = "invoices";

        /// <summary>
        /// Name of the companies container.
        /// </summary>
        /// <seealso cref="Vera.Models.Company"/>
        /// <seealso cref="Vera.Models.Account"/>
        /// <seealso cref="Vera.Models.User"/>
        public string Companies { get; init; } = "companies";

        /// <summary>
        /// Name of the audits container.
        /// </summary>
        public string Audits { get; init; } = "audits";

        /// <summary>
        /// Name of the trails container.
        /// </summary>
        public string Trails { get; init; } = "trails";

        /// <summary>
        /// Name of the chains container. Generic container for chaining all sorts
        /// of documents.
        /// </summary>
        public string Chains { get; init; } = "chains";

        /// <summary>
        /// Name of the periods container.
        /// </summary>
        public string Periods { get; init; } = "periods";

        /// <summary>
        /// Contain different types of documents
        /// <seealso cref="Models.Report"/>
        /// </summary>
        public string Documents { get; init; } = "documents";

        /// <summary>
        /// Contains event logs
        /// </summary>
        public string EventLogs { get; init; } = "eventlogs";
    }   
}