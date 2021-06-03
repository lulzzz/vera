using Vera.Signing;

namespace Vera.Reports
{
    public interface IReportComponentFactory
    {
        IReportSigner CreateReportSigner();
    }
}
