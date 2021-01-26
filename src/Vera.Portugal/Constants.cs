namespace Vera.Portugal
{
    public static class Constants
    {
        /// <summary>
        /// Unique document code. 0 until it has passed regulations.
        /// Ordinance 4.1.4.2.
        /// </summary>
        public const string ATCUD = "0";

        /// <summary>
        /// Default value for CustomerTaxID if non is known.
        /// Ordinance 2.2.3.
        /// </summary>
        public const string DefaultCustomerTaxId = "999999990";

        /// <summary>
        /// Value that is used for a lot of fields if no value is known.
        /// 2.3.2
        /// 2.2.2
        /// 2.2.6.3/4/5/7
        /// 2.2.7.3/4/5/7
        /// </summary>
        public const string UnknownLabel = "Desconhecido";
    }
}