namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.Orders;
    using LearningSystem.Services.Models.ShoppingCart;

    public interface IOrderService
    {
        Task<IEnumerable<OrderListingServiceModel>> AllByUser(string userId);

        Task<int> Create(string userId, PaymentMethod paymentMethod, decimal totalPrice, IEnumerable<CartItemDetailsServiceModel> cartItems);

        Task<OrderListingServiceModel> GetByIdForUser(int id, string userId);
    }
}
