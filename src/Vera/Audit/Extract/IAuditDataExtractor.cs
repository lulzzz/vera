namespace Vera.Audit.Extract
{
    public interface IAuditDataExtractor
    {
        void Extract(Models.Invoice invoice);
    }
}