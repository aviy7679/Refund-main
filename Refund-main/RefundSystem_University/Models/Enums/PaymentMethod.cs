using System.ComponentModel.DataAnnotations;

namespace RefundSystem_University.Models.Enums
{
    public enum PaymentMethod : byte
    {
        [Display(Name = "צ'ק")]
        Cheque = 0,
        [Display(Name = "אשראי")]
        Credit = 1,
        //[Display(Name = "העברה בנקאית")]
        //BankTransfer = 2,
    }
}