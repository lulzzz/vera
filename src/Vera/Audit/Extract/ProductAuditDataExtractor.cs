using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Vera.StandardAuditFileTaxation;
using Invoice = Vera.Models.Invoice;

namespace Vera.Audit.Extract
{
    public class ProductAuditDataExtractor : IAuditDataExtractor
    {
        private readonly IDictionary<string, Product> _products;

        public ProductAuditDataExtractor()
        {
            _products = new Dictionary<string, Product>();
        }

        public void Extract(Invoice invoice)
        {
            var products = invoice.Lines
                .Where(l => l.Product != null)
                .Select(l => l.Product);
            
            foreach (var product in products)
            {
                if (_products.ContainsKey(product.Code))
                {
                    continue;
                }

                _products[product.Code] = new Product
                {
                    Barcode = product.Barcode,
                    Code = product.Code,
                    Description = product.Description,
                    Type = ExtractProductType(product)
                };
            }
        }

        public void Apply(StandardAuditFileTaxation.Audit audit)
        {
            foreach (var entry in _products)
            {
                audit.MasterFiles.Products.Add(entry.Value);
            }
        }
        
        private ProductTypes ExtractProductType(Models.Product product)
        {
            return product.Type switch
            {
                Models.ProductTypes.Goods => ProductTypes.Goods,
                Models.ProductTypes.Service => ProductTypes.Services,
                _ => throw new ArgumentOutOfRangeException(nameof(product), product.Type, "Type does not match a valid value")
            };
        }        
    }
}