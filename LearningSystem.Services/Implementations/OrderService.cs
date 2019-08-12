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

        public async Task<IEnumerable<OrderListingServiceModel>> AllByUserAsync(string userId)
            => await this.mapper
            .ProjectTo<OrderListingServiceModel>(
                this.db
                .Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate))
            .ToListAsync();

        public async Task<bool> CanBeDeletedAsync(int id, string userId)
        {
            // UserOrder not found
            var exists = await this.ExistsAsync(id, userId);
            if (!exists)
            {
                return false;
            }

            var orderCourseIds = await this.db
                .Orders
                .Where(o => o.Id == id)
                .Where(o => o.UserId == userId)
                .SelectMany(o => o.OrderItems.Select(oi => oi.CourseId))
                .ToListAsync();

            var courseStartDates = await this.db
                .Courses
                .Where(c => orderCourseIds.Contains(c.Id)) // db courses
                .Select(c => c.StartDate)
                .ToListAsync();

            // Order contains an item referencing a non-existing course => cannot unsubscribe 
            if (orderCourseIds.Count != courseStartDates.Count)
            {
                return false;
            }

            // Order contains an item referencing a course that has already begun => cannot unsubscribe after start date
            var canUnsubscribeFromAllCourses = !courseStartDates.Any(d => d.HasEnded());

            return canUnsubscribeFromAllCourses;
        }

        public async Task<int> CreateAsync(
            string userId,
            PaymentType paymentMethod,
            decimal totalPrice,
            IEnumerable<CartItemDetailsServiceModel> cartItems)
        {
            var invalidOrderId = int.MinValue;

            var userExists = this.db.Users.Any(u => u.Id == userId);
            if (!userExists
                || !cartItems.Any())
            {
                return invalidOrderId;
            }

            var itemIds = cartItems.Select(i => i.Id);
            var validOrderItems = this.db
                .Courses
                .Where(c => itemIds.Contains(c.Id)) // existing course
                .Where(c => !c.StartDate.HasEnded()) // course has not started
                .Where(c => !c.Students.Any(sc => sc.StudentId == userId)) // user is not enrolled in course
                .Select(c => new OrderItem
                {
                    CourseId = c.Id,
                    CourseName = $"{c.Name} - {c.StartDate.ToDateString()}",
                    Price = c.Price,
                })
                .ToList();

            var hasValidCartCourses = validOrderItems.Count == itemIds.Count();
            var hasValidCartTotalPrice = validOrderItems.Sum(oi => oi.Price) == totalPrice;

            if (!hasValidCartCourses
                || !hasValidCartTotalPrice)
            {
                return invalidOrderId;
            }

            var order = new Order
            {
                UserId = userId,
                TotalPrice = totalPrice,
                OrderItems = validOrderItems,
                PaymentMethod = paymentMethod,
                OrderDate = DateTime.UtcNow,
                Status = Status.Pending
            };

            await this.db.Orders.AddAsync(order);
            await this.db.SaveChangesAsync();

            return order.Id;
        }

        public async Task<bool> ExistsAsync(int id, string userId)
            => await this.db.Orders
            .AnyAsync(o => o.Id == id && o.UserId == userId);

        public async Task<OrderListingServiceModel> GetByIdForUserAsync(int id, string userId)
            => await this.mapper
            .ProjectTo<OrderListingServiceModel>(
                this.db
                .Orders
                .Where(o => o.Id == id)
                .Where(o => o.UserId == userId))
            .FirstOrDefaultAsync();

        public async Task<OrderListingServiceModel> GetInvoiceAsync(string invoiceId)
            => await this.mapper
            .ProjectTo<OrderListingServiceModel>(
                this.db
                .Orders
                .Where(o => o.InvoiceId == invoiceId))
            .FirstOrDefaultAsync();

        public async Task<bool> RemoveAsync(int id, string userId)
        {
            var order = await this.db.Orders.FindAsync(id);
            if (order == null)
            {
                return false;
            }

            var canBeDeleted = await this.CanBeDeletedAsync(id, userId);
            if (!canBeDeleted)
            {
                return false;
            }

            this.db.Orders.Remove(order);
            var result = await this.db.SaveChangesAsync();

            return result > 0;
        }
    }
}
