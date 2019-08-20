namespace University.Data.Models
{
    using System.ComponentModel.DataAnnotations;

    public enum PaymentType
    {
        [Display(Name = "Debit/Credit Card")]
        DebitCreditCard = 0,
        [Display(Name = "Bank Transfer")]
        BankTransfer = 1,
        Voucher = 2,
        Cash = 3
    }
}
