namespace LearningSystem.Services.Models.Orders
{
    using System;
    using System.Collections.Generic;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;

    public class OrderListingServiceModel : IMapFrom<Order>
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public Status Status { get; set; }

        public IEnumerable<OrderItemServiceModel> OrderItems { get; set; }
    }
}
