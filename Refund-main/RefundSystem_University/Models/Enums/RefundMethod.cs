using System.ComponentModel.DataAnnotations;

namespace RefundSystem_University.Models.Enums
{
    public enum RefundMethod : byte
    {
        [Display(Name = "החזר בנקאי")]
        Bank = 0,
        [Display(Name = "החזר בצ'ק")]
        Cheque = 1,
    }
}