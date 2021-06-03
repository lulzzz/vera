using System;
using Vera.Integration.Tests;

namespace Vera.Germany.Integration.Tests
{
    public static class Constants
    {
        public static AccountContext Account => new()
        {
            AccountName = "Store",
            Certification = "DE",
            Configuration =
            {
                {"ApiKey", ""},
                {"ApiSecret", ""},
                {"BaseUrl", ""},
            },
            SupplierSystemId = Guid.NewGuid().ToString()
        };
    }
}
