namespace LearningSystem.Tests.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using LearningSystem.Services;
    using LearningSystem.Services.Implementations;
    using LearningSystem.Services.Models.ShoppingCart;
    using Xunit;

    public class ShoppingCartManagerTest
    {
        private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;

        private const string CartItemsField = "items";

        private const string GetOrAddShoppingCartMethod = "GetOrAddShoppingCart";

        private const string ShoppingCartId = "ShoppingCartId";
        private const string ShoppingCartsField = "shoppingCarts";

        [Fact]
        public void Interfaces_ShouldContainISingletonService()
        {
            // Arrange

            // Act
            var interfaces = typeof(ShoppingCartManager).GetInterfaces();

            // Assert
            Assert.Contains(typeof(ISingletonService), interfaces);
        }

        [Fact]
        public void Interfaces_ShouldContainIShoppingCartManager()
        {
            // Arrange

            // Act
            var interfaces = typeof(ShoppingCartManager).GetInterfaces();

            // Assert
            Assert.Contains(typeof(IShoppingCartManager), interfaces);
        }

        [Fact]
        public void Fields_ShouldContainPrivateShoppingCarts()
        {
            // Arrange

            // Act
            var shoppingCartsInfo = GetShoppingCartsWithReflexion();

            // Assert
            Assert.NotNull(shoppingCartsInfo);
            Assert.Contains(nameof(ConcurrentDictionary<string, ShoppingCart>), shoppingCartsInfo.FieldType.Name);
        }

        [Fact]
        public void Constructors_ShouldContainPublicEmptyConstructor()
        {
            // Arrange

            // Act
            var constructorInfo = typeof(ShoppingCartManager).GetConstructor(Type.EmptyTypes);

            // Assert
            Assert.True(constructorInfo.IsPublic);
            Assert.False(constructorInfo.IsStatic);
        }

        [Fact]
        public void Constructor_ShouldInitializeShoppingCartsAsEmptyConcurrentDictionary()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            // Act
            var cartsFieldInfo = GetShoppingCartsWithReflexion();
            var cartsValue = cartsFieldInfo.GetValue(shoppingCartManager);

            // Assert
            var carts = Assert.IsType<ConcurrentDictionary<string, ShoppingCart>>(cartsValue);
            Assert.Empty(carts);
        }

        [Fact]
        public void Methods_ShouldContainCorrectMethods()
        {
            // Arrange

            // Act
            var methodsInfo = typeof(ShoppingCartManager).GetMethods(BindingFlags.Instance);

            // Assert
            Assert.NotNull(methodsInfo.Where(m => m.IsPublic && m.Name == nameof(ShoppingCartManager.AddItemToCart)));
            Assert.NotNull(methodsInfo.Where(m => m.IsPublic && m.Name == nameof(ShoppingCartManager.EmptyCart)));
            Assert.NotNull(methodsInfo.Where(m => m.IsPublic && m.Name == nameof(ShoppingCartManager.GetCartItems)));
            Assert.NotNull(methodsInfo.Where(m => m.IsPublic && m.Name == nameof(ShoppingCartManager.RemoveItemFromCart)));

            Assert.NotNull(methodsInfo.Where(m => !m.IsPublic && m.Name == GetOrAddShoppingCartMethod));
        }

        [Fact]
        public void GetOrAddShoppingCart_ShouldReturnNewShoppingCartWithEmptyCollectionOfCartItems_GivenInvalidCartId()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var getOrAddShoppingCartInfo = GetOrAddShoppingCartWithReflexion();

            // Act
            var result = getOrAddShoppingCartInfo
                .Invoke(shoppingCartManager, new object[] { ShoppingCartId });

            // Assert
            var resultShoppingCart = Assert.IsType<ShoppingCart>(result);

            var shoppingCartsInfo = GetShoppingCartsWithReflexion();
            var shoppingCartsValue = shoppingCartsInfo.GetValue(shoppingCartManager);
            var shoppingCartsDict = Assert.IsType<ConcurrentDictionary<string, ShoppingCart>>(shoppingCartsValue);

            // ShoppingCartId added to carts dictionary keys
            Assert.Contains(ShoppingCartId, shoppingCartsDict.Keys);

            // Empty new shopping cart added for ShoppingCartId in carts dictionary
            var dictShoppingCart = shoppingCartsDict[ShoppingCartId];
            Assert.Equal(dictShoppingCart, resultShoppingCart);

            // Cart items
            var cartItemsInfo = GetCartItemsWithReflexion();
            var resultCartItemsValue = cartItemsInfo.GetValue(resultShoppingCart);
            var resultCartItems = Assert.IsAssignableFrom<IList<CartItem>>(resultCartItemsValue);
            Assert.Empty(resultCartItems);
        }

        [Fact]
        public void GetOrAddShoppingCart_ShouldReturnCorrectShoppingCartWithCartItems_GivenExistingCartId()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = new List<CartItem> { new CartItem { CourseId = 1 }, new CartItem { CourseId = 2 } };

            // Add items to shopping cart
            var getOrAddShoppingCartInfo = GetOrAddShoppingCartWithReflexion();
            var cartItemsInfo = GetCartItemsWithReflexion();

            var initialShoppingCart = getOrAddShoppingCartInfo
                .Invoke(shoppingCartManager, new object[] { ShoppingCartId }) as ShoppingCart;

            cartItemsInfo.SetValue(initialShoppingCart, items);

            // Act
            var resultShoppingCart = getOrAddShoppingCartInfo
                .Invoke(shoppingCartManager, new object[] { ShoppingCartId }) as ShoppingCart;

            // Assert
            // Cart items
            var resultCartItems = cartItemsInfo.GetValue(resultShoppingCart) as IList<CartItem>;
            Assert.NotEmpty(resultCartItems);
            Assert.Equal(items, resultCartItems);

            var shoppingCartsInfo = GetShoppingCartsWithReflexion();
            var shoppingCartsDict = shoppingCartsInfo.GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;

            // CartId exists in carts dictionary
            Assert.Contains(ShoppingCartId, shoppingCartsDict.Keys);
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
        public void GetCartItems_ShouldReturnCorrectData_GivenCartItemsInShoppingCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = new List<CartItem> { new CartItem { CourseId = 1 }, new CartItem { CourseId = 2 } };

            // Add items to cart
            var getOrAddShoppingCartInfo = GetOrAddShoppingCartWithReflexion();
            var cartItemsInfo = GetCartItemsWithReflexion();

            var shoppingCart = getOrAddShoppingCartInfo
                .Invoke(shoppingCartManager, new object[] { ShoppingCartId }) as ShoppingCart;

            cartItemsInfo.SetValue(shoppingCart, items);

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

            var items = new List<CartItem> { new CartItem { CourseId = 1 }, new CartItem { CourseId = 2 } };

            // Add items to cart
            var getOrAddShoppingCartInfo = GetOrAddShoppingCartWithReflexion();
            var cartItemsInfo = GetCartItemsWithReflexion();

            var shoppingCart = getOrAddShoppingCartInfo
                .Invoke(shoppingCartManager, new object[] { ShoppingCartId }) as ShoppingCart;

            cartItemsInfo.SetValue(shoppingCart, items);

            // Act
            var result1 = shoppingCartManager.GetCartItems(ShoppingCartId);
            result1 = new List<CartItem>();

            var result2 = shoppingCartManager.GetCartItems(ShoppingCartId);

            // Assert
            Assert.NotEmpty(result2);
            Assert.Equal(items, result2);
        }

        private static MethodInfo GetOrAddShoppingCartWithReflexion()
            => typeof(ShoppingCartManager)
            .GetMethods(Flags)
            .Where(f => f.Name == GetOrAddShoppingCartMethod)
            .FirstOrDefault();

        private static FieldInfo GetShoppingCartsWithReflexion()
            => typeof(ShoppingCartManager)
            .GetFields(Flags)
            .Where(f => f.Name == ShoppingCartsField)
            .FirstOrDefault();

        private static FieldInfo GetCartItemsWithReflexion()
            => typeof(ShoppingCart)
            .GetFields(Flags)
            .Where(f => f.Name == CartItemsField)
            .FirstOrDefault();

        private IShoppingCartManager InitializeShoppingCartManager()
            => new ShoppingCartManager();
    }
}
