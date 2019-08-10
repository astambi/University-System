namespace LearningSystem.Services.Models.Orders
{
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class OrderItemServiceModel : IMapFrom<OrderItem>
    {
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        public decimal Price { get; set; }
    }
}
