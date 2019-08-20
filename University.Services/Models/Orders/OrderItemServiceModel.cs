namespace University.Services.Models.Orders
{
    using University.Common.Mapping;
    using University.Data.Models;

    public class OrderItemServiceModel : IMapFrom<OrderItem>
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public decimal Price { get; set; }
    }
}
