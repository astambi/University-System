namespace University.Web.Models.ShoppingCart
{
    using System.ComponentModel.DataAnnotations;
    using University.Data.Models;

    public class CheckoutFormModel
    {
        [Display(Name = "Payment method")]
        [Required]
        public PaymentType? PaymentMethod { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
