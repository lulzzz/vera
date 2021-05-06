using Vera.Grpc;

namespace Vera.Host.Mapping
{
    public static class ReportExtensions
    {
        public static PaymentReport Pack(this Vera.Models.RegisterReport.PaymentReport r)
        {
            return new PaymentReport
            {
                Amount = r.Amount,
                Category = r.PaymentCategory.Map(),
                Count = r.Count
            };
        }

        public static TaxReport Pack(this Vera.Models.RegisterReport.TaxesReport r)
        {
            return new TaxReport
            {
                Amount = r.Amount,
                Category = r.TaxesCategory.Map(),
                Rate = r.TaxRate
            };
        }

        public static ProductReport Pack(this Vera.Models.RegisterReport.ProductReport r)
        {
            return new ProductReport
            {
                Amount = r.Amount,
                Count = r.Count,
                Type = r.Type.Map()
            };
        }

        public static EmployeePaymentsReport Pack(this Vera.Models.RegisterReport.EmployeePaymentsReport r)
        {
            return new EmployeePaymentsReport
            {
                Employee = new EmployeeReport
                {
                    FirstName = r.Employee.FirstName,
                    LastName = r.Employee.LastName,
                    SystemId = r.Employee.SystemId
                },
                Payment = r.Payment.Pack()
            };
        }
    }
}
