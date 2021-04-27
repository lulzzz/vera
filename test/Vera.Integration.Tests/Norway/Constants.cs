using System;

namespace Vera.Integration.Tests.Norway
{
    public static class Constants
    {
        private const string Pk = @"-----BEGIN RSA PRIVATE KEY-----
MIICXQIBAAKBgQC96HMXSV2MuX4VmVbj5y0oZeCZzjXDaZLvi0U+S1sAt/VfTRT7
cOgHm2VbLo+o5nx24UbIxH0osATZBUTYZCEiwRnSDp0cyse1MmbsfmCd9KYWWyc7
I9I63Jn0lwkCJWp+hU2SMrumRpKqJ0Lza+T4S5matp0wikQr/RZorNjN4QIDAQAB
AoGAPirwMjlUJJM8kTmHVkgBYm4nXnJA612OOlivLDti6RNPggkryzwk2Qin33eY
k8QQDqKkl2irSDyG+bxd0zDEH5nwCmT59pvTQ1t36E6mtPgg4wfpPyy86sCi1ecJ
UwoZjdpDnwfAWDOjgCgZiu1tr9L9uw7hG8fn+/99kSSmxTkCQQDm0ISO2CASWc0X
Y2gKU//ZZXBymDjnq+VvKBRAyO2lpfc6LnL0op7NnbDdHcUJO21nzX9pr+wJqLaN
W/feWYpXAkEA0qFD5KqfDYJ21VxMRLZ+SNAD5zgIGx6VajpcIFMZfmZ6H0OShKBY
KQlbYS22baIClrHawyDU/jARO8eC2JG2hwJAIRd8Kc6qqnbdhKDn5bMtV0nH2WYh
onVuq4UfgjpMeBdXXqwSJyi5g9k75jfCbBRtFxjLT6e9O5VItvOckfBceQJBAIx1
G+hF21DP+lyncvizVZ1Kkf/DfqxPBcZT6pFnuO1weumUTwWAQ6oB4lz4ddnAGsfR
DJfosgBbn3Jkxh2TdcsCQQDhjs8VIbJITJtCvsfmi0SykOyuDvFZlEWqy/io12ge
tq4HEcmINDkh3fy0/V5XRqzAmGlH6dgxPMgEdddzdRrl
-----END RSA PRIVATE KEY-----";

        public static AccountContext Account => new()
        {
            CompanyName = "Pelican Theory",
            AccountName = "Store",
            Certification = "NO",
            Configuration =
            {
                {"PrivateKey", Pk},
                {"PrivateKeyVersion", "1"},
            },
            SupplierSystemId = Guid.NewGuid().ToString()
        };
    }
}
