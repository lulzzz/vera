using Vera.Models;

namespace Vera
{
    public interface IInvoiceBucketGenerator
    {
        /// <summary>
        /// Generates a string that is the same for every invoice that would
        /// belong to the same 'bucket'. E.g. all invoices go in to the same bucket
        /// or credit and debit invoices have their own buckets.
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns></returns>
        string Generate(Invoice invoice);
    }
}