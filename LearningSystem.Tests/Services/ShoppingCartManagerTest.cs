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
        private const BindingFlags FlagsPrivate = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        private const BindingFlags FlagPublic = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

        private const int CourseIdExisting = 10;
        private const int CourseIdNotFound = 200;

        private const string ShoppingCartId = "ShoppingCartId";

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

        [Fact]
        public void Fields_ShouldContainPrivateConcurrentDictionaryOfStringToShoppingCart()
        {
            // Arrange

            // Act
            var cartsDictionaryInfo = GetCartsDictionaryWithReflexion();

            // Assert
            Assert.NotNull(cartsDictionaryInfo);
        }

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
        public void Constructor_ShouldInitializePrivateFieldCartsDictionaryAsEmptyConcurrentDictionaryOfStringToShoppingCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            // Act
            var cartsObj = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager);

            // Assert
            var cartsDictionary = Assert.IsType<ConcurrentDictionary<string, ShoppingCart>>(cartsObj);
            Assert.Empty(cartsDictionary);
        }

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
        public void GetOrAddShoppingCart_ShouldBePrivate_WithReturnTypeShoppingCart_WithParamStringCartId()
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
            var resultCart = Assert.IsType<ShoppingCart>(result);

            // ShoppingCartId added to carts dictionary keys
            var cartsDictObj = GetCartsDictionaryWithReflexion().GetValue(shoppingCartManager);
            var cartsDict = Assert.IsType<ConcurrentDictionary<string, ShoppingCart>>(cartsDictObj);
            Assert.Contains(ShoppingCartId, cartsDict.Keys);

            // Empty new cart added for ShoppingCartId in carts dictionary
            var dictValue = cartsDict[ShoppingCartId];
            Assert.Equal(dictValue, resultCart);

            // Cart items
            var cartItemsObj = GetCartItemsWithReflexion().GetValue(resultCart);
            var cartItems = Assert.IsAssignableFrom<IList<CartItem>>(cartItemsObj);
            Assert.Empty(cartItems);
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
            var resultCart = Assert.IsType<ShoppingCart>(result);

            // ShoppingCartId added to carts dictionary keys
            var cartsDictObj = GetCartsDictionaryWithReflexion().GetValue(shoppingCartManager);
            var cartsDict = Assert.IsType<ConcurrentDictionary<string, ShoppingCart>>(cartsDictObj);
            Assert.Contains(ShoppingCartId, cartsDict.Keys);

            // Empty new cart added for ShoppingCartId in carts dictionary
            var dictValue = cartsDict[ShoppingCartId];
            Assert.Equal(dictValue, resultCart);

            // Cart items
            var cartItemsObj = GetCartItemsWithReflexion().GetValue(resultCart);
            var cartItems = Assert.IsAssignableFrom<IList<CartItem>>(cartItemsObj);
            Assert.Equal(items, cartItems);
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

            var resultPreliminary = shoppingCartManager.GetCartItems(ShoppingCartId);
            resultPreliminary = new List<CartItem>();

            // Act
            var result = shoppingCartManager.GetCartItems(ShoppingCartId);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(items, result);
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
            // ShoppingCartId added to carts dictionary keys
            var cartsDict = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;
            Assert.Contains(ShoppingCartId, cartsDict.Keys);

            // Cart items
            var cart = cartsDict[ShoppingCartId];
            var cartItems = GetCartItemsWithReflexion().GetValue(cart) as IList<CartItem>;
            Assert.Equal(items, cartItems);
        }

        [Fact]
        public void AddItemToCart_ShouldAddCorrectItem_GivenItemIdNotFoundInCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = this.GetItems();
            var itemsCountBefore = items.Count;

            this.PrepareShoppingCartWithItems(shoppingCartManager, items);

            // Act
            shoppingCartManager.AddItemToCart(ShoppingCartId, CourseIdNotFound);

            // Assert
            var cartsDict = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;
            Assert.Contains(ShoppingCartId, cartsDict.Keys);

            // Cart items
            var cart = cartsDict[ShoppingCartId];
            var cartItems = GetCartItemsWithReflexion().GetValue(cart) as IList<CartItem>;
            Assert.Contains(CourseIdNotFound, cartItems.Select(i => i.CourseId));

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
            var cartsDict = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;
            Assert.Contains(ShoppingCartId, cartsDict.Keys);

            // Cart items
            var cart = cartsDict[ShoppingCartId];
            var cartItems = GetCartItemsWithReflexion().GetValue(cart) as IList<CartItem>;
            Assert.Empty(cartItems);
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
            var cartsDict = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;
            Assert.Contains(ShoppingCartId, cartsDict.Keys);

            // Cart items
            var cart = cartsDict[ShoppingCartId];
            var cartItems = GetCartItemsWithReflexion().GetValue(cart) as IList<CartItem>;
            Assert.Equal(items, cartItems);
        }

        [Fact]
        public void RemoveItemToCart_ShouldRemoveCorrectItem_GivenExisitngItemIdInCart()
        {
            // Arrange
            var shoppingCartManager = this.InitializeShoppingCartManager();

            var items = this.GetItems();
            var itemsCountBefore = items.Count;

            this.PrepareShoppingCartWithItems(shoppingCartManager, items);

            // Act
            shoppingCartManager.RemoveItemFromCart(ShoppingCartId, CourseIdExisting);

            // Assert
            var cartsDict = GetCartsDictionaryWithReflexion()
                .GetValue(shoppingCartManager) as ConcurrentDictionary<string, ShoppingCart>;
            Assert.Contains(ShoppingCartId, cartsDict.Keys);

            // Cart items
            var cart = cartsDict[ShoppingCartId];
            var cartItems = GetCartItemsWithReflexion().GetValue(cart) as IList<CartItem>;
            Assert.DoesNotContain(CourseIdExisting, cartItems.Select(i => i.CourseId));

            var itemsCountAfter = cartItems.Count;
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

        private IShoppingCartManager InitializeShoppingCartManager()
            => new ShoppingCartManager();

        private List<CartItem> GetItems()
           => new List<CartItem> { new CartItem { CourseId = CourseIdExisting }, new CartItem { CourseId = 2 } };

        private void PrepareShoppingCartWithItems(IShoppingCartManager shoppingCartManager, IEnumerable<CartItem> items)
        {
            var shoppingCart = GetOrAddCartMethodWithReflexion()
                .Invoke(shoppingCartManager, new object[] { ShoppingCartId }) as ShoppingCart;

            GetCartItemsWithReflexion()
                .SetValue(shoppingCart, items);
        }
    }
}
