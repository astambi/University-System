namespace University.Services.Implementations
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using University.Services.Models.ShoppingCart;

    public class ShoppingCartManager : IShoppingCartManager, ISingletonService
    {
        private readonly ConcurrentDictionary<string, ShoppingCart> shoppingCarts;

        public ShoppingCartManager()
        {
            this.shoppingCarts = new ConcurrentDictionary<string, ShoppingCart>();
        }

        public void AddItemToCart(string shoppingCartId, int courseId)
            => this.GetOrAddShoppingCart(shoppingCartId)
            .AddItem(courseId);

        public void EmptyCart(string shoppingCartId)
            => this.GetOrAddShoppingCart(shoppingCartId)
            .Clear();

        public IEnumerable<CartItem> GetCartItems(string shoppingCartId)
            => new List<CartItem>(this.GetOrAddShoppingCart(shoppingCartId).Items);

        public void RemoveItemFromCart(string shoppingCartId, int courseId)
            => this.GetOrAddShoppingCart(shoppingCartId)
            .RemoveItem(courseId);

        private ShoppingCart GetOrAddShoppingCart(string id)
            => this.shoppingCarts.GetOrAdd(id, new ShoppingCart());
    }
}
