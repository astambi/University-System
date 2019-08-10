namespace LearningSystem.Services.Implementations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using LearningSystem.Common.Infrastructure.Extensions;
    using LearningSystem.Data;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Orders;
    using LearningSystem.Services.Models.ShoppingCart;
    using Microsoft.EntityFrameworkCore;

    public class OrderService : IOrderService
    {
        private readonly LearningSystemDbContext db;
        private readonly IMapper mapper;

        public OrderService(
            LearningSystemDbContext db,
            IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<IEnumerable<OrderListingServiceModel>> AllByUser(string userId)
            => await this.mapper
            .ProjectTo<OrderListingServiceModel>(
                this.db
                .Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate))
            .ToListAsync();

        public async Task<int> Create(
            string userId,
            PaymentMethod paymentMethod,
            decimal totalPrice,
            IEnumerable<CartItemDetailsServiceModel> cartItems)
        {
            var invalidOrderId = int.MinValue;

            var userExists = this.db.Users.Any(u => u.Id == userId);
            if (!userExists)
            {
                return invalidOrderId;
            }

            var itemIds = cartItems.Select(i => i.Id);
            var orderItems = this.db
                .Courses
                .Where(c => itemIds.Contains(c.Id))
                .Where(c => !c.StartDate.HasEnded()) // course has not started
                .Select(c => new OrderItem
                {
                    CourseId = c.Id,
                    CourseName = $"{c.Name} - {c.StartDate.ToDateString()}",
                    Price = c.Price,
                })
                .ToList();

            if (orderItems.Sum(oi => oi.Price) != totalPrice)
            {
                return invalidOrderId;
            }

            var order = new Order
            {
                UserId = userId,
                TotalPrice = totalPrice,
                OrderItems = orderItems,
                PaymentMethod = paymentMethod,
                OrderDate = DateTime.UtcNow,
                Status = Status.Pending
            };

            await this.db.Orders.AddAsync(order);
            await this.db.SaveChangesAsync();

            return order.Id;
        }

        public async Task<OrderListingServiceModel> GetByIdForUser(int id, string userId)
            => await this.mapper
            .ProjectTo<OrderListingServiceModel>(
                this.db
                .Orders
                .Where(o => o.Id == id)
                .Where(o => o.UserId == userId))
            .FirstOrDefaultAsync();
    }
}
