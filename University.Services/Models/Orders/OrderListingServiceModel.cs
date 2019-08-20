namespace University.Services.Models.Orders
{
    using System;
    using System.Collections.Generic;
    using University.Common.Mapping;
    using University.Data.Models;

    public class OrderListingServiceModel : IMapFrom<Order>
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
