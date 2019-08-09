namespace LearningSystem.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using LearningSystem.Data.Models;
    using LearningSystem.Services.Models.ShoppingCart;

    public interface IOrderService
    {
        Task<int> Create(
            string userId,
            PaymentMethod paymentMethod,
            decimal totalPrice,
            IEnumerable<CartItemDetailsServiceModel> cartItems);
    }
}
