// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo
namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// See 4.8.11. Totalizers and counters readout
  /// Keep it simple until we know how many of them we actually need
  /// </summary>
  public enum TotalizersReadoutEnum
  {
    /// See 4.8.11.1 Transaction totalizers
    traDOTKEYtot,
    traDOTKEYtotDOTKEYv,
    traDOTKEYtot_diff,
    traDOTKEYtot_diffDOTKEYv,
    traDOTKEYtot_pack,
    traDOTKEYtot_pack_d,
    traDOTKEYvoid,
    traDOTKEYdisc,
    traDOTKEYuplf,
    traDOTKEYvoidDOTKEYv,
    traDOTKEYdiscDOTKEYv,
    traDOTKEYuplfDOTKEYv,
    traDOTKEYtot_disc,
    traDOTKEYtot_uplf,
    traDOTKEYtot_discDOTKEYv,
    traDOTKEYtot_uplfDOTKEYv,
    /// See 4.8.11.1 Daily totalizers
    dayDOTKEYtot,
    dayDOTKEYtotDOTKEYv,
    dayDOTKEYtot_canc,
    dayDOTKEYvat,
    dayDOTKEYvatDOTKEYv,
    dayDOTKEYrcpt_tot,
    dayDOTKEYrcpt_totDOTKEYv,
    dayDOTKEYrcpt_tot_canc,
    dayDOTKEYrcpt_vat,
    dayDOTKEYrcpt_vatDOTKEYv,
    dayDOTKEYinv_tot,
    dayDOTKEYinv_totDOTKEYv,
    dayDOTKEYinv_tot_canc,
    dayDOTKEYinv_vat,
    dayDOTKEYinv_vatDOTKEYv,
    dayDOTKEYrcpt_payDOTKEYp,
    dayDOTKEYrcpt_change,
    dayDOTKEYrcpt_changeDOTKEYp,
    dayDOTKEYrcpt_void,
    dayDOTKEYrcpt_disc,
    dayDOTKEYrcpt_uplf,
    dayDOTKEYrcpt_tot_disc,
    dayDOTKEYrcpt_tot_uplf,
    dayDOTKEYlast_tot,
    dayDOTKEYlast_rcpt_tot,
    dayDOTKEYlast_inv_tot
  }
}