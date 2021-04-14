using Vera.Dependencies;
using Vera.Models;

namespace Vera.Reports
{
    public class ReportBucketGenerator : IBucketGenerator<RegisterReport>
    {
        public string Generate(RegisterReport report)
        {
            return $"{report.SupplierId}-{report.ReportType}";
        }
    }
}
