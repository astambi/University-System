namespace University.Data.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class OrderItem
    {
        public int Id { get; set; }

        public int CourseId { get; set; }

        [Required]
        public string CourseName { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue,
            ErrorMessage = DataConstants.NegativeNumber)]
        public decimal Price { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; }
    }
}
