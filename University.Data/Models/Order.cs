namespace University.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Order
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue,
            ErrorMessage = DataConstants.NegativeNumber)]
        public decimal TotalPrice { get; set; }

        public PaymentType PaymentMethod { get; set; }

        public Status Status { get; set; }

        [Required]
        [MaxLength(DataConstants.InvoiceIdMaxLength,
            ErrorMessage = DataConstants.StringMaxLength)]
        public string InvoiceId { get; set; } = Guid.NewGuid().ToString().Replace("-", string.Empty);

        [Required]
        public string UserId { get; set; }

        [Required]
        [StringLength(DataConstants.UserNameMaxLength,
            ErrorMessage = DataConstants.StringMinMaxLength,
            MinimumLength = DataConstants.UserNameMinLength)]
        public string UserName { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
