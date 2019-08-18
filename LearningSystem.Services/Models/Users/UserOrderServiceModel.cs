namespace LearningSystem.Services.Models.Users
{
    using System;
    using System.Collections.Generic;
    using LearningSystem.Common.Mapping;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Orders;

    public class UserOrderServiceModel : IMapFrom<Order>
    {
        public int Id { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }

        public PaymentType PaymentMethod { get; set; }

        public Status Status { get; set; }

        public string InvoiceId { get; set; }

        public IEnumerable<OrderItemServiceModel> OrderItems { get; set; }
    }
}
