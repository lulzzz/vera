using System;

namespace Vera.Norway
{
    public interface IDateProvider
    {
        DateTime Now { get; }
    }

    public class RealLifeDateProvider : IDateProvider
    {
        public DateTime Now => DateTime.UtcNow;
    }
}
