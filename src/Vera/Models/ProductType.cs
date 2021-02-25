namespace Vera.Models
{
    public enum ProductType
    {
        /// <summary>
        /// Unspecified what kind of product this is. Generic fallback.
        /// </summary>
        Other,
        
        /// <summary>
        /// Generic type of products that are physically handed over to the customer.
        /// </summary>
        Goods,
        
        /// <summary>
        /// Card that can be used in another store or online to make a purchase.
        /// </summary>
        GiftCard
    }
}