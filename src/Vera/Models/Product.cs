namespace Vera.Models
{
    public class Product
    {
        public string SystemId { get; set; }
        public ProductTypes Type { get; set; }
        public string Code { get; set; }
        public string Barcode { get; set; }
        public string Description { get; set; }
    }
}