using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Vera.Dependencies;
using Vera.Dependencies.Handlers;
using Vera.Models;
using Vera.Signing;
using Vera.Stores;

namespace Vera.Reports
{
    public class ReportPersistenceHandler : HandlerChain<RegisterReport>
    {
        private readonly ILogger<ReportPersistenceHandler> _logger;
        private readonly IChainStore _chainStore;
        private readonly IReportStore _reportStore;
        private readonly IPackageSigner _signer;
        private readonly IBucketGenerator<RegisterReport> _reportBucketGenerator;

        public ReportPersistenceHandler(ILogger<ReportPersistenceHandler> logger,
            IChainStore chainStore,
            IReportStore reportStore,
            IPackageSigner signer,
            IBucketGenerator<RegisterReport> reportBucketGenerator)
        {
            _logger = logger;
            _chainStore = chainStore;
            _reportStore = reportStore;
            _signer = signer;
            _reportBucketGenerator = reportBucketGenerator;
        }

        public override async Task Handle(RegisterReport report)
        {
            var bucket = _reportBucketGenerator.Generate(report);
            var chainContext = new ChainContext(report.Account.Id, bucket);

            // Get last stored report based on the bucket for the report
            var last = await _chainStore.Last(chainContext);

            report.Sequence = last.NextSequence;
            report.Number = last.NextSequence.ToString();
            report.Signature = await _signer.Sign(new Package(report, last.Signature));

            await _reportStore.Store(report);

            try
            {
                await last.Append(report.Signature);
            }
            catch (Exception chainException)
            {
                // Failed to append to the chain, means we have to
                // rollback the report because it's not stored in a chain
                _logger.LogError(chainException,
                    $"failed to append to chain {chainContext}, deleting created report {report.Id}");

                try
                {
                    await _reportStore.Delete(report);
                    _logger.LogInformation("successfully removed report, chain has been restored");
                }
                catch (Exception ex)
                {
                    // TODO: this means the chain is now in an invalid state
                    // ^ how do we recover from this?
                    _logger.LogError(ex,
                        "failed to delete report after appending to the chain failed");
                }
            }

            await base.Handle(report);
        }
    }
}
