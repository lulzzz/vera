namespace Vera.StandardAuditFileTaxation
{
  public class Product
  {
    /// <summary>
    /// Code of the article. Usually used for the article number etc.
    /// </summary>
    public string Code { get; set; }

    public ProductTypes Type { get; set; }

    /// <summary>
    /// Aggregation of similar products.
    /// </summary>
    public string Group { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// Classification for import/export.
    /// </summary>
    public string CommodityCode { get; set; }

    /// <summary>
    /// EAN or other code of the product.
    /// SAF-T name is: ProductNumberCode.
    /// </summary>
    public string Barcode { get; set; }

    // TODO(kevin): valutation methods

    /// <summary>
    /// Unit of measure base that's used for this product.
    /// </summary>
    public string Unit { get; set; }
  }
}