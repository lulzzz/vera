using Vera.Models;

namespace Vera.Germany.Fiskaly
{
    public static class FiskalyHelper
    {
        public static string MapTax(TaxesCategory taxes) => taxes switch
            {
                TaxesCategory.High => "NORMAL",
                TaxesCategory.Low => "REDUCED_1",
                _ => "NULL"
            };
    }
}
