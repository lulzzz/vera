using System;
using System.Threading.Tasks;
using Vera.Concurrency;
using Vera.Dependencies;
using Vera.Models;
using Vera.Stores;

namespace Vera.Periods
{
    public class PeriodOpener : IPeriodOpener
    {
        private readonly ILocker _locker;
        private readonly IPeriodStore _periodStore;
        private readonly IDateProvider _dateProvider;

        public PeriodOpener(
            ILocker locker,
            IPeriodStore periodStore,
            IDateProvider dateProvider
        )
        {
            _locker = locker;
            _periodStore = periodStore;
            _dateProvider = dateProvider;
        }

        public async Task<Period> Open(Guid supplierId)
        {
            await using (await _locker.Lock(supplierId.ToString(), TimeSpan.FromSeconds(5)))
            {
                var currentPeriod = await _periodStore.GetOpenPeriodForSupplier(supplierId);

                if (currentPeriod != null)
                {
                    return currentPeriod;
                }

                var period = new Period
                {
                    Opening = _dateProvider.Now,
                    SupplierId = supplierId
                };

                await _periodStore.Store(period);

                return period;
            }
        }
    }
}
