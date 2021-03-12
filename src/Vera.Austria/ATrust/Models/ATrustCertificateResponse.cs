using System.Collections.Generic;
using Newtonsoft.Json;

namespace Vera.Austria.ATrust.Models
{
  public sealed class ATrustCertificateResponse
  {
    [JsonProperty("Signaturzertifikat")]
    public string SigningCertificate { get; set; }

    [JsonProperty("Zertifizierungsstellen")]
    public ICollection<string> CertificateAuthorities { get; set; }

    [JsonProperty("Zertifikatsseriennummer")]
    public string CertificateSerialNumber { get; set; }

    [JsonProperty("ZertifikatsseriennummerHex")]
    public string CertificateSerialNumberHex { get; set; }

    [JsonProperty("alg")]
    public string Algorithm { get; set; }
  }
}