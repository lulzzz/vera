namespace Vera.Audit.Extract
{
    public interface IAuditDataExtractor
    {
        void Extract(Models.Invoice invoice);
        void Apply(StandardAuditFileTaxation.Audit audit);
    }
}