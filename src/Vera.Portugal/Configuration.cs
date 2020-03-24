using System.ComponentModel.DataAnnotations;

namespace Vera.Portugal
{
    public class Configuration
    {
        [Required]
        [Display(
            Name = "PrivateKey",
            GroupName = "Security",
            Description = "RSA private key to use for signing the invoices"
        )]
        public byte[] PrivateKey { get; set; }

        [Required]
        [Display(
            Name = "ProductCompanyId",
            GroupName = "General",
            Description = "???"
        )]
        public string ProductCompanyTaxId { get; set; }

        [Required]
        [Display(
            Name = "SocialCapital",
            GroupName = "General",
            Description = "Required to be printed on all the receipts. Social capital of the company."
        )]
        public decimal SocialCapital { get; set; }        
    }
}