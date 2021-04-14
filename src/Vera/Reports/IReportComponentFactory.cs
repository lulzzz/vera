using Vera.Signing;

namespace Vera.Reports
{
    public interface IReportComponentFactory
    {
        IPackageSigner CreatePackageSigner();
    }
}
