namespace University.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using University.Services.Models.ShoppingCart;
    using Xunit;

    public class ShoppingCartTest
    {
        private const BindingFlags FlagsPrivate = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        private const BindingFlags FlagPublic = BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly;

        private const int CourseIdExisting = 10;
        private const int CourseIdNotFound = 200;

        // Fields
        [Fact]
        public void Fields_ShouldContainPrivateListOfCartItems()
        {
            // Arrange

            // Act
            var itemsFieldInfo = GetCartItemsWithReflexion();

            // Assert
            Assert.NotNull(itemsFieldInfo);
        }

        // Constructors
        [Fact]
        public void Constructors_ShouldContainPublicEmptyConstructor()
        {
            // Arrange

            // Act
            var constructorInfo = typeof(ShoppingCart)
                .GetConstructor(Type.EmptyTypes);

            // Assert
            Assert.True(constructorInfo.IsPublic);
            Assert.False(constructorInfo.IsStatic);
        }

        [Fact]
        public void Constructor_ShouldInitializePrivateFieldItems_AsEmptyListOfCartItems()
        {
            // Arrange
            var shoppingCart = this.InitializeShoppingCart();

            // Act
            var itemsObj = GetCartItemsWithReflexion().GetValue(shoppingCart);

            // Assert
            var items = Assert.IsAssignableFrom<IList<CartItem>>(itemsObj);
            Assert.Empty(items);
        }

        // Properties
        [Fact]
        public void Items_ShouldBePublicProperty_OfTypeIEnumerableOfCartItem()
        {
            // Arrange

            // Act
            var itemsPropertyInfo = typeof(ShoppingCart)
                .GetProperties(FlagPublic)
                .Where(p => typeof(IEnumerable<CartItem>).IsAssignableFrom(p.PropertyType))
                .FirstOrDefault();

            // Assert
            Assert.NotNull(itemsPropertyInfo);
        }

        [Fact]
        public void Items_ShouldReturnEmptyCollection_GivenEmptyShoppingCart()
        {
            // Arrange
            var shoppingCart = this.InitializeShoppingCart();

            // Act
            var result = shoppingCart.Items;

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CartItem>>(result);
            Assert.Empty(result);
        }

        [Fact]
        public void Items_ShouldReturnCorrectCollection_GivenItemsInShoppingCart()
        {
            // Arrange
            var shoppingCart = this.InitializeShoppingCart();

            var items = this.GetItems();
            this.PrepareCartItems(shoppingCart, items);

            // Act
            var result = shoppingCart.Items;

            // Assert
            Assert.IsAssignableFrom<IEnumerable<CartItem>>(result);
            Assert.Equal(items, result);
        }

        [Fact]
        public void Items_ShouldBeEncapsulated()
        {
            // Arrange
            var shoppingCart = this.InitializeShoppingCart();

            var items = this.GetItems();
            this.PrepareCartItems(shoppingCart, items);

            // Act
            var result = shoppingCart.Items;

            // Assert
            Assert.NotSame(items, result);
        }

        // Private Methods
        [Fact]
        public void GetItemById_ShouldBePrivateMethod_WithReturnTypeCartItem_WithParamIntCourseId()
        {
            // Arrange

            // Act
            var getItemByIdMethodInfo = GetItemByIdWithReflexion();

            // Assert
            Assert.NotNull(getItemByIdMethodInfo);
        }

        [Fact]
        public void GetItemById_ShouldReturnNull_GivenNonExisingCourseId()
        {
            // Arrange
            var shoppingCart = this.InitializeShoppingCart();

            // Act
            var result = GetItemByIdWithReflexion()
                .Invoke(shoppingCart, new object[] { CourseIdNotFound });

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetItemById_ShouldReturnCorrectData_GivenExisingCourseId()
        {
            // Arrange
            var shoppingCart = this.InitializeShoppingCart();

            var items = this.GetItems();
            this.PrepareCartItems(shoppingCart, items);

            // Act
            var result = GetItemByIdWithReflexion()
                .Invoke(shoppingCart, new object[] { CourseIdExisting });

            // Assert
            Assert.NotNull(result);

            var resultItem = Assert.IsType<CartItem>(result);
            Assert.Contains(resultItem, items);
        }

        // Public Methods
        [Fact]
        public void Methods_ShouldContainCorrectPublicMethods()
        {
            // Arrange

            // Act
            var methodsInfo = typeof(ShoppingCart)
                .GetMethods(FlagPublic);

            // Assert
            Assert.NotNull(methodsInfo.Where(m => m.Name == nameof(ShoppingCart.AddItem)));
            Assert.NotNull(methodsInfo.Where(m => m.Name == nameof(ShoppingCart.Clear)));
            Assert.NotNull(methodsInfo.Where(m => m.Name == nameof(ShoppingCart.RemoveItem)));
        }

        [Fact]
        public void AddItem_ShouldNotChangeItems_GivenExistingItemIdInCart()
        {
            // Arrange
            var shoppingCart = this.InitializeShoppingCart();

            var items = this.GetItems();
            this.PrepareCartItems(shoppingCart, items);

            // Act
            shoppingCart.AddItem(CourseIdExisting);

            // Assert
            var cartItems = GetCartItemsWithReflexion().GetValue(shoppingCart) as IList<CartItem>;
            Assert.Equal(items, cartItems);
        }

        [Fact]
        public void AddItem_ShouldAddCorrectItem_GivenItemIdNotFoundInCart()
        {
            // Arrange
            var shoppingCart = this.InitializeShoppingCart();

            var items = this.GetItems();
            this.PrepareCartItems(shoppingCart, items);

            var itemsCountBefore = items.Count;

            // Act
            shoppingCart.AddItem(CourseIdNotFound);

            // Assert
            var cartItems = GetCartItemsWithReflexion().GetValue(shoppingCart) as IList<CartItem>;
            Assert.Contains(CourseIdNotFound, cartItems.Select(i => i.CourseId));

            var itemsCountAfter = items.Count;
            Assert.Equal(1, itemsCountAfter - itemsCountBefore);
        }

        [Fact]
        public void Clear_ShouldClearItemsCollection()
        {
            // Arrange
            var shoppingCart = this.InitializeShoppingCart();

            var items = this.GetItems();
            this.PrepareCartItems(shoppingCart, items);

            // Act
            shoppingCart.Clear();

            // Assert
            var cartItems = GetCartItemsWithReflexion().GetValue(shoppingCart) as IList<CartItem>;
            Assert.Empty(cartItems);
        }

        [Fact]
        public void RemoveItem_ShouldNotChangeItemsCollection_GivenItemIdNotFoundInCart()
        {
            // Arrange
            var shoppingCart = this.InitializeShoppingCart();

            var items = this.GetItems();
            this.PrepareCartItems(shoppingCart, items);

            // Act
            shoppingCart.RemoveItem(CourseIdNotFound);

            // Assert
            var cartItems = GetCartItemsWithReflexion().GetValue(shoppingCart) as IList<CartItem>;
            Assert.Equal(items, cartItems);
        }

        [Fact]
        public void RemoveItem_ShouldRemoveCorrectItem_GivenExisitngItemIdInCart()
        {
            // Arrange
            var shoppingCart = this.InitializeShoppingCart();

            var items = this.GetItems();
            this.PrepareCartItems(shoppingCart, items);

            var itemsCountBefore = items.Count;

            // Act
            shoppingCart.RemoveItem(CourseIdExisting);

            // Assert
            var cartItems = GetCartItemsWithReflexion().GetValue(shoppingCart) as IList<CartItem>;
            Assert.DoesNotContain(CourseIdExisting, cartItems.Select(i => i.CourseId));

            var itemsCountAfter = cartItems.Count;
            Assert.Equal(-1, itemsCountAfter - itemsCountBefore);
        }

        private static FieldInfo GetCartItemsWithReflexion()
            => typeof(ShoppingCart)
            .GetFields(FlagsPrivate)
            .Where(f => typeof(IEnumerable<CartItem>).IsAssignableFrom(f.FieldType))
            .FirstOrDefault();

        private static MethodInfo GetItemByIdWithReflexion()
            => typeof(ShoppingCart)
            .GetMethods(FlagsPrivate)
            .Where(m => m.GetParameters().Any(p => p.ParameterType == typeof(int)))
            .Where(m => m.ReturnType == typeof(CartItem))
            .FirstOrDefault();

        private List<CartItem> GetItems()
          => new List<CartItem> { new CartItem { CourseId = CourseIdExisting }, new CartItem { CourseId = 2 } };

        private void PrepareCartItems(ShoppingCart shoppingCart, IEnumerable<CartItem> items)
            => GetCartItemsWithReflexion().SetValue(shoppingCart, items);

        private ShoppingCart InitializeShoppingCart()
            => new ShoppingCart();
    }
}
