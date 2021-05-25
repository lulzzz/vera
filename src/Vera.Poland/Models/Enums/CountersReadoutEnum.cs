// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Vera.Poland.Models.Enums
{
  /// <summary>
  /// See 4.8.11. Totalizers and counters readout
  /// Keep it simple until we know how many of them we actually need
  /// </summary>
  public enum CountersReadoutEnum
  {
    /// See 4.8.11.1 Transaction counters
    tra_nDOTKEYpay,
    tra_nDOTKEYpayDOTKEYp,
    tra_nDOTKEYpay_r,
    tra_nDOTKEYpay_rDOTKEYp,
    tra_nDOTKEYitem_ln,
    tra_nDOTKEYvoid,
    tra_nDOTKEYdisc,
    tra_nDOTKEYuplf,
    tra_nDOTKEYvoid_r,
    tra_nDOTKEYdisc_r,
    tra_nDOTKEYuplf_r,
    tra_nDOTKEYtot_disc,
    tra_nDOTKEYtot_uplf,
    tra_nDOTKEYtot_disc_r,
    tra_nDOTKEYtot_uplf_r,
    /// See 4.8.11.1 Daily counters
    day_nDOTKEYrcpt_open,
    day_nDOTKEYrcpt_clos,
    day_nDOTKEYrcpt_canc,
    day_nDOTKEYinv_open,
    day_nDOTKEYinv_clos,
    day_nDOTKEYinv_canc,
    day_nDOTKEYprintouts,
    day_nDOTKEYprintouts_async,
    day_nDOTKEYprintouts_fis,
    day_nDOTKEYprintouts_nfis,
    day_nDOTKEYrcpt_itemln,
    day_nDOTKEYrcpt_void,
    day_nDOTKEYrcpt_disc,
    day_nDOTKEYrcpt_uplf,
    day_nDOTKEYrcpt_void_r,
    day_nDOTKEYrcpt_disc_r,
    day_nDOTKEYrcpt_uplf_r,
    day_nDOTKEYrcpt_tot_disc,
    day_nDOTKEYrcpt_tot_uplf,
    day_nDOTKEYrcpt_tot_disc_r,
    day_nDOTKEYrcpt_tot_uplf_r,
    /// See 4.8.11.1 Non-reset accumulators
    grnDOTKEYtot,
    grnDOTKEYtotDOTKEYv,
    grnDOTKEYvat,
    grnDOTKEYvatDOTKEYv,
    grnDOTKEYrcpt_tot,
    grnDOTKEYrcpt_totDOTKEYv,
    grnDOTKEYrcpt_vat,
    grnDOTKEYrcpt_vatDOTKEYv,
    grnDOTKEYinv_tot,
    grnDOTKEYinv_totDOTKEYv,
    grnDOTKEYinv_vat,
    grnDOTKEYinv_vatDOTKEYv,
    /// See 4.8.11.1 Non-reset counters
    grn_nDOTKEYrcpt_open,
    grn_nDOTKEYrcpt_clos,
    grn_nDOTKEYrcpt_canc,
    grn_nDOTKEYinv_open,
    grn_nDOTKEYinv_clos,
    grn_nDOTKEYprintouts,
    grn_nDOTKEYprintouts_fis,
    grn_nDOTKEYprintouts_nfis,
    grn_nDOTKEYfm_vat,
    grn_nDOTKEYfm_head,
    grn_nDOTKEYfm_serv,
    grn_nDOTKEYday_open,
    grn_nDOTKEYday_clos
  }
}