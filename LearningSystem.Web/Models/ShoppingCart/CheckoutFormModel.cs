namespace LearningSystem.Web.Models.ShoppingCart
{
    using System.ComponentModel.DataAnnotations;
    using LearningSystem.Data.Models;

    public class CheckoutFormModel
    {
        [Display(Name = "Payment method")]
        [Required]
        public PaymentType? PaymentMethod { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
