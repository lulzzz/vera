namespace Vera.Azure
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
        /// <seealso cref="Vera.Models.Company"/>
        /// <seealso cref="Vera.Models.Account"/>
        /// <seealso cref="Vera.Models.User"/>
        public string Companies { get; set; } = "companies";

        /// <summary>
        /// Name of the audits container.
        /// </summary>
        public string Audits { get; set; } = "audits";

        /// <summary>
        /// Name of the trails container.
        /// </summary>
        public string Trails { get; set; } = "trails";

        /// <summary>
        /// Name of the chains container. Generic container for chaining all sorts
        /// of documents.
        /// </summary>
        public string Chains { get; set; } = "chains";
    }
}