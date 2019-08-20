namespace University.Tests.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using University.Services;
    using University.Services.Implementations;
    using University.Services.Models.ShoppingCart;
    using Xunit;

    public class ShoppingCartManagerTest
    {
        private const BindingFlags FlagsPrivate = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        private const BindingFlags FlagPublic = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

        private const int CourseIdExisting = 10;
        private const int CourseIdNotFound = 200;

        private const string ShoppingCartId = "ShoppingCartId";

        // Interfaces
        [Fact]
        public void Interfaces_ShouldContainISingletonService()
        {
            // Arrange

            // Act
            var interfaces = typeof(ShoppingCartManager)
                .GetInterfaces();

            // Assert
            Assert.Contains(typeof(ISingletonService), interfaces);
        }

        [Fact]
        public void Interfaces_ShouldContainIShoppingCartManager()
        {
            // Arrange

            // Act
            var interfaces = typeof(ShoppingCartManager)
                .GetInterfaces();

            // Assert
            Assert.Contains(typeof(IShoppingCartManager), interfaces);
        }

        // Fields
        [Fact]
        public void Fields_ShouldContainPrivateCarts_OfTypeConcurrentDictionaryOfStringToShoppingCart()
        {
            // Arrange

            // Act
            var cartsFieldInfo = GetCartsDictionaryWithReflexion();

            // Assert
            Assert.NotNull(cartsFieldInfo);
        }

        // Constructors
        [Fact]
        public void Constructors_ShouldContainPublicEmptyConstructor()
        {
            // Arrange

            // Act
            var constructorInfo = typeof(ShoppingCartManager)
                .GetConstructor(Type.EmptyTypes);

            // Assert
            Assert.True(constructorInfo.IsPublic);
            Assert.False(constructorInfo.IsStatic);
        }

        [Fact]
        public void Constructor_ShouldInitializePrivateFieldCarts_AsEmptyConcurrentDictionaryOfStringToShoppingCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            // Act
            var cartsObj = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager);

            // Assert
            var carts = Assert.IsType<ConcurrentDictionary<string, ShoppingCart>>(cartsObj);
            Assert.Empty(carts);
        }

        // Private Methods
        [Fact]
        public void GetOrAddShoppingCart_ShouldBePrivateMethod_WithReturnTypeShoppingCart_WithParamStringCartId()
        {
            // Arrange

            // Act
            var getOrAddCartMethodInfo = GetOrAddCartMethodWithReflexion();

            // Assert
            Assert.NotNull(getOrAddCartMethodInfo);
        }

        [Fact]
        public void GetOrAddShoppingCart_ShouldReturnNewCartWithEmptyCollectionOfCartItems_GivenNonExisingCartId()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            // Act
            var result = GetOrAddCartMethodWithReflexion()
                .Invoke(shoppingCartManager, new object[] { ShoppingCartId });

            // Assert
            var carts = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;

            // ShoppingCartId added to Carts dictionary
            Assert.Contains(ShoppingCartId, carts.Keys);

            // Returned Cart corresponds to ShoppingCartId in carts dictionary
            var resultCart = Assert.IsType<ShoppingCart>(result);
            var cartFromDictionary = carts[ShoppingCartId];

            Assert.Equal(cartFromDictionary, resultCart);
            Assert.Same(cartFromDictionary, resultCart);

            // Returned Cart is empty
            Assert.Empty(cartFromDictionary.Items);
            Assert.Empty(resultCart.Items);
        }

        [Fact]
        public void GetOrAddShoppingCart_ShouldReturnCorrectCartWithExistingCartItems_GivenExistingCartId()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = this.GetItems();
            this.PrepareShoppingCartWithItems(shoppingCartManager, items);

            // Act
            var result = GetOrAddCartMethodWithReflexion()
                .Invoke(shoppingCartManager, new object[] { ShoppingCartId });

            // Assert
            var carts = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;

            // ShoppingCartId added to Carts dictionary
            Assert.Contains(ShoppingCartId, carts.Keys);

            // Returned Cart corresponds to ShoppingCartId in carts dictionary
            var resultCart = Assert.IsType<ShoppingCart>(result);
            var cartFromDictionary = carts[ShoppingCartId];

            Assert.Equal(cartFromDictionary, resultCart);
            Assert.Same(cartFromDictionary, resultCart);

            // Cart items
            Assert.Equal(items, resultCart.Items);
            Assert.Equal(items, cartFromDictionary.Items);
        }

        // Public Methods
        [Fact]
        public void Methods_ShouldContainCorrectPublicMethods()
        {
            // Arrange

            // Act
            var methodsInfo = typeof(ShoppingCartManager)
                .GetMethods(FlagPublic);

            // Assert
            Assert.NotNull(methodsInfo.Where(m => m.Name == nameof(ShoppingCartManager.AddItemToCart)));
            Assert.NotNull(methodsInfo.Where(m => m.Name == nameof(ShoppingCartManager.EmptyCart)));
            Assert.NotNull(methodsInfo.Where(m => m.Name == nameof(ShoppingCartManager.GetCartItems)));
            Assert.NotNull(methodsInfo.Where(m => m.Name == nameof(ShoppingCartManager.RemoveItemFromCart)));
        }

        [Fact]
        public void GetCartItems_ShouldBePublic_WithReturnTypeIEnumerableOfShoppingCartItems_WithParamStringCartId()
        {
            // Arrange

            // Act
            var getCartItemsMethodInfo = typeof(ShoppingCartManager)
                .GetMethods(FlagPublic)
                .Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(string)))
                .Where(m => typeof(IEnumerable<CartItem>).IsAssignableFrom(m.ReturnType))
                .FirstOrDefault();

            // Assert
            Assert.NotNull(getCartItemsMethodInfo);
        }

        [Fact]
        public void GetCartItems_ShouldReturnEmptyCollection_GivenEmptyShoppingCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            // Act
            var result = shoppingCartManager.GetCartItems(ShoppingCartId);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CartItem>>(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetCartItems_ShouldReturnCorrectCollection_GivenCartItemsInShoppingCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = this.GetItems();
            this.PrepareShoppingCartWithItems(shoppingCartManager, items);

            // Act
            var result = shoppingCartManager.GetCartItems(ShoppingCartId);

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CartItem>>(result);
            Assert.Equal(items, result);
        }

        [Fact]
        public void GetCartItems_ShouldBeEncapsulated()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = this.GetItems();
            this.PrepareShoppingCartWithItems(shoppingCartManager, items);

            // Act
            var result = shoppingCartManager.GetCartItems(ShoppingCartId);

            // Assert
            Assert.NotSame(items, result);
        }

        [Fact]
        public void AddItemToCart_ShouldNotChangeItems_GivenExistingItemIdInCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = this.GetItems();
            this.PrepareShoppingCartWithItems(shoppingCartManager, items);

            // Act
            shoppingCartManager.AddItemToCart(ShoppingCartId, CourseIdExisting);

            // Assert
            var carts = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;

            Assert.Contains(ShoppingCartId, carts.Keys);
            var cart = carts[ShoppingCartId];

            Assert.Equal(items, cart.Items);
        }

        [Fact]
        public void AddItemToCart_ShouldAddCorrectItem_GivenItemIdNotFoundInCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = this.GetItems();
            this.PrepareShoppingCartWithItems(shoppingCartManager, items);

            var itemsCountBefore = items.Count;

            // Act
            shoppingCartManager.AddItemToCart(ShoppingCartId, CourseIdNotFound);

            // Assert
            var carts = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;

            Assert.Contains(ShoppingCartId, carts.Keys);
            var cart = carts[ShoppingCartId];

            Assert.Contains(CourseIdNotFound, cart.Items.Select(i => i.CourseId));

            var itemsCountAfter = items.Count;
            Assert.Equal(1, itemsCountAfter - itemsCountBefore);
        }

        [Fact]
        public void EmptyCart_ShouldClearItemsFromCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = this.GetItems();
            this.PrepareShoppingCartWithItems(shoppingCartManager, items);

            // Act
            shoppingCartManager.EmptyCart(ShoppingCartId);

            // Assert
            var carts = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;

            Assert.Contains(ShoppingCartId, carts.Keys);
            var cart = carts[ShoppingCartId];

            Assert.Empty(cart.Items);
        }

        [Fact]
        public void RemoveItemToCart_ShouldNotChangeItemsCollection_GivenItemIdNotFoundInCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = this.GetItems();
            this.PrepareShoppingCartWithItems(shoppingCartManager, items);

            // Act
            shoppingCartManager.RemoveItemFromCart(ShoppingCartId, CourseIdNotFound);

            // Assert
            var carts = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;

            Assert.Contains(ShoppingCartId, carts.Keys);
            var cart = carts[ShoppingCartId];

            Assert.Equal(items, cart.Items);
        }

        [Fact]
        public void RemoveItemToCart_ShouldRemoveCorrectItem_GivenExisitngItemIdInCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = this.GetItems();
            this.PrepareShoppingCartWithItems(shoppingCartManager, items);

            var itemsCountBefore = items.Count;

            // Act
            shoppingCartManager.RemoveItemFromCart(ShoppingCartId, CourseIdExisting);

            // Assert
            var carts = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;

            Assert.Contains(ShoppingCartId, carts.Keys);
            var cart = carts[ShoppingCartId];

            Assert.DoesNotContain(CourseIdExisting, cart.Items.Select(i => i.CourseId));

            var itemsCountAfter = cart.Items.Count();
            Assert.Equal(-1, itemsCountAfter - itemsCountBefore);
        }

        private static MethodInfo GetOrAddCartMethodWithReflexion()
            => typeof(ShoppingCartManager)
            .GetMethods(FlagsPrivate)
            .Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(string)))
            .Where(m => m.ReturnType == typeof(ShoppingCart))
            .FirstOrDefault();

        private static FieldInfo GetCartsDictionaryWithReflexion()
            => typeof(ShoppingCartManager)
            .GetFields(FlagsPrivate)
            .Where(f => typeof(ConcurrentDictionary<string, ShoppingCart>).IsAssignableFrom(f.FieldType))
            .FirstOrDefault();

        private static FieldInfo GetCartItemsWithReflexion()
            => typeof(ShoppingCart)
            .GetFields(FlagsPrivate)
            .Where(f => typeof(IEnumerable<CartItem>).IsAssignableFrom(f.FieldType))
            .FirstOrDefault();

        private List<CartItem> GetItems()
           => new List<CartItem> { new CartItem { CourseId = CourseIdExisting }, new CartItem { CourseId = 2 } };

        private void PrepareShoppingCartWithItems(IShoppingCartManager shoppingCartManager, IEnumerable<CartItem> items)
        {
            var shoppingCart = GetOrAddCartMethodWithReflexion()
                .Invoke(shoppingCartManager, new object[] { ShoppingCartId }) as ShoppingCart;

            GetCartItemsWithReflexion()
                .SetValue(shoppingCart, items);
        }

        private IShoppingCartManager InitializeShoppingCartManager()
            => new ShoppingCartManager();
    }
}
