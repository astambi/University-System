namespace University.Tests.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using University.Data;
    using University.Data.Models;
    using University.Services;
    using University.Services.Implementations;
    using University.Services.Models.Orders;
    using University.Services.Models.ShoppingCart;
    using Xunit;

    public class OrderServiceTest
    {
        private const int CourseIdEnrolled = 1;
        private const int CourseIdValid = 2;
        private const int CourseIdInvalid = 3;
        private const int CourseIdStarted = 4;
        private const int CourseIdFuture = 2000;

        private const string InvoiceValid = "InvoiceValid";
        private const string InvoiceInvalid = "InvoiceInvalid";

        private const PaymentType PaymentMethod = PaymentType.DebitCreditCard;

        private const int OrderIdValid = 2;
        private const int OrderIdInvalid = 20;

        private const string UserIdValid = "UserValid";
        private const string UserIdInvalid = "UserInvalid";
        private const string UserName = "User Name";

        [Fact]
        public async Task AllByUserAsync_ShouldReturnEmptyCollection_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareUserOrdersWithInvoice();
            var orderService = this.InitializeOrderService(db);

            // Act
            var result = await orderService.AllByUserAsync(UserIdInvalid);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task AllByUserAsync_ShouldReturnCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareUserOrdersWithInvoice();
            var orderService = this.InitializeOrderService(db);

            var expected = db.Orders
                .Where(o => o.UserId == UserIdValid)
                .Select(o => new OrderWithItems { Order = o, OrderItems = o.OrderItems.ToList() })
                .ToList();

            // Act
            var result = await orderService.AllByUserAsync(UserIdValid);

            // Assert
            Assert.NotEmpty(result);
            Assert.IsAssignableFrom<IEnumerable<OrderListingServiceModel>>(result);

            this.AssertOrderedByDateDesc(expected, result);

            foreach (var resultItem in result)
            {
                var expectedItem = expected
                    .Where(o => o.Order.Id == resultItem.Id)
                    .FirstOrDefault();

                this.AssertOrder(expectedItem.Order, resultItem);
                this.AssertOrderItems(expectedItem.OrderItems, resultItem.OrderItems.ToList());
            }
        }

        [Fact]
        public async Task CanBeDeletedAsync_ShouldReturnFalse_GivenInvalidUserOrder()
        {
            // Arrange
            var db = await this.PrepareOrderBasic();
            var orderService = this.InitializeOrderService(db);

            // Act
            var resultInvalidOrder = await orderService.CanBeDeletedAsync(OrderIdInvalid, UserIdValid);
            var resultInvalidUser = await orderService.CanBeDeletedAsync(OrderIdValid, UserIdInvalid);

            // Assert
            Assert.False(resultInvalidOrder);
            Assert.False(resultInvalidUser);
        }

        [Fact]
        public async Task CanBeDeletedAsync_ShouldReturnFalse_GivenInvalidOrderCourse()
        {
            // Arrange
            var db = await this.PrepareOrderWithNonExistingCourse();
            var orderService = this.InitializeOrderService(db);

            // Act
            var result = await orderService.CanBeDeletedAsync(OrderIdValid, UserIdValid);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanBeDeletedAsync_ShouldReturnFalse_GivenOrderCourseThatHasAlreadyStarted()
        {
            // Arrange
            var db = await this.PrepareOrderWithCourseThatHasAlreadyStarted();
            var orderService = this.InitializeOrderService(db);

            // Act
            var result = await orderService.CanBeDeletedAsync(OrderIdValid, UserIdValid);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task CanBeDeletedAsync_ShouldReturnTrue_GivenAllOrderCoursesHaveNotStarted()
        {
            // Arrange
            var db = await this.PrepareOrderWithAllFutureCourses();
            var orderService = this.InitializeOrderService(db);

            // Act
            var result = await orderService.CanBeDeletedAsync(OrderIdValid, UserIdValid);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task CreateAsync_ShouldNotSaveOrder_GivenInvalidUser()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            var orderService = this.InitializeOrderService(db);

            // Act
            var resultOrderId = await orderService.CreateAsync(
                UserIdInvalid,
                It.IsAny<PaymentType>(),
                It.IsAny<int>(),
                It.IsAny<List<CartItemDetailsServiceModel>>());

            var orderSaved = await db.Orders.FindAsync(resultOrderId);
            var ordersSavedCount = db.Orders.Count();

            // Assert
            Assert.True(resultOrderId < 0);
            Assert.Null(orderSaved);
            Assert.Equal(0, ordersSavedCount);
        }

        [Fact]
        public async Task CreateAsync_ShouldNotSaveOrder_GivenEmptyCartItems()
        {
            // Arrange
            var db = Tests.InitializeDatabase();
            await db.Users.AddAsync(new User { Id = UserIdValid });
            await db.SaveChangesAsync();

            var orderService = this.InitializeOrderService(db);

            var cartItemsEmpty = new List<CartItemDetailsServiceModel>();

            // Act
            var resultOrderId = await orderService.CreateAsync(
                UserIdValid,
                It.IsAny<PaymentType>(),
                It.IsAny<int>(),
                cartItemsEmpty);

            var orderSaved = await db.Orders.FindAsync(resultOrderId);
            var ordersSavedCount = db.Orders.Count();

            // Assert
            Assert.True(resultOrderId < 0);
            Assert.Null(orderSaved);
            Assert.Equal(0, ordersSavedCount);
        }

        [Fact]
        public async Task CreateAsync_ShouldNotSaveOrder_GivenInvalidShoppingCartCourse()
        {
            // Arrange
            var db = await this.PrepareUserAndFutureCourses();
            var orderService = this.InitializeOrderService(db);

            var cartItems = new List<CartItemDetailsServiceModel>
            {
                new CartItemDetailsServiceModel { Id = CourseIdInvalid, Price = 3, Name = "Some name" }
            };
            var cartTotal = cartItems.Sum(i => i.Price); // 3

            // Act
            var resultOrderId = await orderService.CreateAsync(
                UserIdValid,
                It.IsAny<PaymentType>(),
                cartTotal,
                cartItems);

            var orderSaved = await db.Orders.FindAsync(resultOrderId);
            var ordersSavedCount = db.Orders.Count();

            // Assert
            Assert.True(resultOrderId < 0);
            Assert.Null(orderSaved);
            Assert.Equal(0, ordersSavedCount);
        }

        [Fact]
        public async Task CreateAsync_ShouldNotSaveOrder_GivenAnyCourseHasStarted()
        {
            // Arrange
            var db = await this.PrepareUserAndCoursesWithCourseThatHasAlreadyStarted();
            var orderService = this.InitializeOrderService(db);

            var cartItems = new List<CartItemDetailsServiceModel>
            {
                new CartItemDetailsServiceModel { Id = CourseIdStarted, Price = 1, Name = "Some name" }, // course has started
                new CartItemDetailsServiceModel { Id = CourseIdFuture, Price = 2, Name = "Some other name" },
            };
            var cartTotal = cartItems.Sum(i => i.Price); // 3

            // Act
            var resultOrderId = await orderService.CreateAsync(
                UserIdValid,
                PaymentMethod,
                cartTotal,
                cartItems);

            var orderSaved = await db.Orders.FindAsync(resultOrderId);
            var ordersSavedCount = db.Orders.Count();

            // Assert
            Assert.True(resultOrderId < 0);
            Assert.Equal(0, ordersSavedCount);
            Assert.Null(orderSaved);
        }

        [Fact]
        public async Task CreateAsync_ShouldNotSaveOrder_GivenAnyEnrolledShoppingCartCourse()
        {
            // Arrange
            var db = await this.PrepareUserWithEnrolledFutureCourses();
            var orderService = this.InitializeOrderService(db);

            var cartItems = new List<CartItemDetailsServiceModel>
            {
                new CartItemDetailsServiceModel { Id = CourseIdEnrolled, Price = 1, Name = "Some name" }, // course enrolled
                new CartItemDetailsServiceModel { Id = CourseIdFuture, Price = 2, Name = "Some other name" },
            };
            var cartTotal = cartItems.Sum(i => i.Price); // 3

            // Act
            var resultOrderId = await orderService.CreateAsync(
                UserIdValid,
                PaymentMethod,
                cartTotal,
                cartItems);

            var orderSaved = await db.Orders.FindAsync(resultOrderId);
            var ordersSavedCount = db.Orders.Count();

            // Assert
            Assert.True(resultOrderId < 0);
            Assert.Equal(0, ordersSavedCount);
            Assert.Null(orderSaved);
        }

        [Fact]
        public async Task CreateAsync_ShouldNotSaveOrder_GivenTotalPriceMismatch()
        {
            // Arrange
            var db = await this.PrepareUserAndFutureCourses();
            var orderService = this.InitializeOrderService(db);

            var cartItems = new List<CartItemDetailsServiceModel>
            {
                new CartItemDetailsServiceModel { Id = CourseIdValid, Price = 1, Name = "Some name" },
                new CartItemDetailsServiceModel { Id = CourseIdFuture, Price = 2, Name = "Some other name" },
            };
            var cartTotalInvalid = 300;

            // Act
            var resultOrderId = await orderService.CreateAsync(
                UserIdValid,
                PaymentMethod,
                cartTotalInvalid,
                cartItems);

            var orderSaved = await db.Orders.FindAsync(resultOrderId);
            var ordersSavedCount = db.Orders.Count();

            // Assert
            Assert.True(resultOrderId < 0);
            Assert.Equal(0, ordersSavedCount);
            Assert.Null(orderSaved);
        }

        [Fact]
        public async Task CreateAsync_ShouldSaveCorrectData_GivenValidCartItemsAndTotalPrice()
        {
            // Arrange
            var db = await this.PrepareUserAndFutureCourses();
            var orderService = this.InitializeOrderService(db);

            var cartItems = new List<CartItemDetailsServiceModel>
            {
                new CartItemDetailsServiceModel { Id = CourseIdValid, Price = 1, Name = "Some name" },
                new CartItemDetailsServiceModel { Id = CourseIdFuture, Price = 2, Name = "Some other name" },
            };
            var cartTotalValid = cartItems.Sum(i => i.Price); // 3

            // Act
            var resultOrderId = await orderService.CreateAsync(
                UserIdValid,
                PaymentMethod,
                cartTotalValid,
                cartItems);

            var resultOrderSaved = await db.Orders.FindAsync(resultOrderId);
            var resultOrdersCount = db.Orders.Count();

            // Assert
            Assert.True(resultOrderId >= 0);
            Assert.Equal(1, resultOrdersCount);

            Assert.NotNull(resultOrderSaved);

            Assert.Equal(UserIdValid, resultOrderSaved.UserId);
            Assert.Equal(UserName, resultOrderSaved.UserName);
            Assert.Equal(Status.Completed, resultOrderSaved.Status);
            Assert.Equal(PaymentMethod, resultOrderSaved.PaymentMethod);
            Assert.NotNull(resultOrderSaved.InvoiceId);
            Assert.Equal(cartTotalValid, resultOrderSaved.TotalPrice);

            Assert.Equal(cartItems.Count, resultOrderSaved.OrderItems.Count);

            var resultOrderItems = resultOrderSaved.OrderItems.ToList();
            for (var i = 0; i < resultOrderItems.Count; i++)
            {
                var expectedCartItem = cartItems[i];
                var resultOrderItem = resultOrderItems[i];

                var expectedCourseName = db
                    .Courses
                    .Where(c => c.Id == resultOrderItem.CourseId)
                    .Select(c => c.Name)
                    .FirstOrDefault();

                Assert.Equal(resultOrderId, resultOrderItem.OrderId);
                Assert.Equal(expectedCartItem.Id, resultOrderItem.CourseId);
                Assert.Equal(expectedCartItem.Price, resultOrderItem.Price);
                Assert.StartsWith(expectedCourseName, resultOrderItem.CourseName);
            }

            var currentDateTime = DateTime.UtcNow;
            Assert.Equal(currentDateTime.Year, resultOrderSaved.OrderDate.Year);
            Assert.Equal(currentDateTime.Month, resultOrderSaved.OrderDate.Month);
            Assert.Equal(currentDateTime.Day, resultOrderSaved.OrderDate.Day);
            Assert.Equal(currentDateTime.Minute, resultOrderSaved.OrderDate.Minute);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnFalse_GivenInvalidUserOrder()
        {
            // Arrange
            var db = await this.PrepareOrderBasic();
            var orderService = this.InitializeOrderService(db);

            // Act
            var resultInvalidOrder = await orderService.ExistsAsync(OrderIdInvalid, UserIdValid);
            var resultInvalidUser = await orderService.ExistsAsync(OrderIdValid, UserIdInvalid);

            // Assert
            Assert.False(resultInvalidOrder);
            Assert.False(resultInvalidUser);
        }

        [Fact]
        public async Task ExistsAsync_ShouldReturnTrue_GivenValidUserOrder()
        {
            // Arrange
            var db = await this.PrepareOrderBasic();
            var orderService = this.InitializeOrderService(db);

            // Act
            var resultValid = await orderService.ExistsAsync(OrderIdValid, UserIdValid);

            // Assert
            Assert.True(resultValid);
        }

        [Fact]
        public async Task GetByIdForUserAsync_ShouldReturnNull_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareUserOrdersWithInvoice();
            var orderService = this.InitializeOrderService(db);

            // Act
            var resultUserInvalid = await orderService.GetByIdForUserAsync(OrderIdValid, UserIdInvalid);
            var resultOrderInvalid = await orderService.GetByIdForUserAsync(OrderIdInvalid, UserIdValid);

            // Assert
            Assert.Null(resultUserInvalid);
            Assert.Null(resultOrderInvalid);
        }

        [Fact]
        public async Task GetByIdForUserAsync_ShouldReturnCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareUserOrdersWithInvoice();
            var orderService = this.InitializeOrderService(db);

            var expected = await db
               .Orders
               .Where(o => o.Id == OrderIdValid)
               .Where(o => o.UserId == UserIdValid)
               .Select(o => new OrderWithItems { Order = o, OrderItems = o.OrderItems.ToList() })
               .FirstOrDefaultAsync();

            // Act
            var result = await orderService.GetByIdForUserAsync(OrderIdValid, UserIdValid);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OrderListingServiceModel>(result);

            this.AssertOrder(expected.Order, result);
            this.AssertOrderItems(expected.OrderItems, result.OrderItems.ToList());
        }

        [Fact]
        public async Task GetInvoiceAsync_ShouldReturnNull_GivenInvalidInput()
        {
            // Arrange
            var db = await this.PrepareUserOrdersWithInvoice();
            var orderService = this.InitializeOrderService(db);

            // Act
            var result = await orderService.GetInvoiceAsync(InvoiceInvalid);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetInvoiceAsync_ShouldReturnCorrectData_GivenValidInput()
        {
            // Arrange
            var db = await this.PrepareUserOrdersWithInvoice();
            var orderService = this.InitializeOrderService(db);

            var expected = await db
                .Orders
                .Where(o => o.InvoiceId == InvoiceValid)
                .Select(o => new OrderWithItems { Order = o, OrderItems = o.OrderItems.ToList() })
                .FirstOrDefaultAsync();

            // Act
            var result = await orderService.GetInvoiceAsync(InvoiceValid);

            // Assert
            Assert.NotNull(result);
            Assert.IsType<OrderListingServiceModel>(result);

            this.AssertOrder(expected.Order, result);
            this.AssertOrderItems(expected.OrderItems, result.OrderItems.ToList());
        }

        [Fact]
        public async Task RemoveAsync_ShouldNotRemoteOrder_GivenInvalidUserOrder()
        {
            // Arrange
            var db = await this.PrepareOrderBasic();
            var orderService = this.InitializeOrderService(db);

            var countBefore = db.Orders.Count();

            // Act
            var resultInvalidOrder = await orderService.RemoveAsync(OrderIdInvalid, UserIdValid);
            var resultInvalidUser = await orderService.RemoveAsync(OrderIdValid, UserIdInvalid);
            var countAfter = db.Orders.Count();

            // Assert
            Assert.False(resultInvalidOrder);
            Assert.False(resultInvalidUser);
            Assert.Equal(countBefore, countAfter);
        }

        [Fact]
        public async Task RemoveAsync_ShouldNotRemoteOrder_GivenInvalidCourse()
        {
            // Arrange
            var db = await this.PrepareOrderWithNonExistingCourse();
            var orderService = this.InitializeOrderService(db);

            var countBefore = db.Orders.Count();

            // Act
            var result = await orderService.RemoveAsync(OrderIdValid, UserIdValid);
            var countAfter = db.Orders.Count();

            // Assert
            Assert.False(result);
            Assert.Equal(countBefore, countAfter);
        }

        [Fact]
        public async Task RemoveAsync_ShouldNotRemoteOrder_GivenAnyOrderCourseHasStarted()
        {
            // Arrange
            var db = await this.PrepareOrderWithCourseThatHasAlreadyStarted();
            var orderService = this.InitializeOrderService(db);

            var countBefore = db.Orders.Count();

            // Act
            var result = await orderService.RemoveAsync(OrderIdValid, UserIdValid);
            var countAfter = db.Orders.Count();

            // Assert
            Assert.False(result);
            Assert.Equal(countBefore, countAfter);
        }

        [Fact]
        public async Task RemoveAsync_ShouldRemoteOrder_GivenAllOrderCoursesHaveNotStarted()
        {
            // Arrange
            var db = await this.PrepareOrderWithAllFutureCourses();
            var orderService = this.InitializeOrderService(db);

            var countBefore = db.Orders.Count();

            // Act
            var result = await orderService.RemoveAsync(OrderIdValid, UserIdValid);
            var orderDeleted = await db.Orders.FindAsync(OrderIdValid);
            var countAfter = db.Orders.Count();

            // Assert
            Assert.True(result);
            Assert.Null(orderDeleted);
            Assert.Equal(countBefore - 1, countAfter);
        }

        private void AssertOrder(Order expectedOrder, OrderListingServiceModel result)
        {
            Assert.Equal(expectedOrder.Id, result.Id);
            Assert.Equal(expectedOrder.OrderDate, result.OrderDate);
            Assert.Equal(expectedOrder.TotalPrice, result.TotalPrice);
            Assert.Equal(expectedOrder.PaymentMethod, result.PaymentMethod);
            Assert.Equal(expectedOrder.Status, result.Status);
            Assert.Equal(expectedOrder.InvoiceId, result.InvoiceId);
        }

        private void AssertOrderItems(List<OrderItem> expectedOrderItems, List<OrderItemServiceModel> resultOrderItems)
        {
            Assert.Equal(expectedOrderItems.Count(), resultOrderItems.Count());

            for (var i = 0; i < resultOrderItems.Count; i++)
            {
                var actualItem = resultOrderItems[i];
                var expectedItem = expectedOrderItems[i];

                Assert.Equal(actualItem.CourseId, expectedItem.CourseId);
                Assert.Equal(actualItem.CourseName, expectedItem.CourseName);
                Assert.Equal(actualItem.Price, expectedItem.Price);
            }
        }

        private void AssertOrderedByDateDesc(IEnumerable<OrderWithItems> expected, IEnumerable<OrderListingServiceModel> result)
        {
            var expectedOrderedIds = expected
                .OrderByDescending(o => o.Order.OrderDate)
                .Select(o => o.Order.Id)
                .ToList();

            var resultIds = result
                .Select(o => o.Id)
                .ToList();

            Assert.Equal(expectedOrderedIds, resultIds);
        }

        private async Task<UniversityDbContext> PrepareOrderBasic()
        {
            var db = Tests.InitializeDatabase();
            await db.Orders.AddAsync(new Order { Id = OrderIdValid, UserId = UserIdValid });
            await db.Users.AddAsync(new User { Id = UserIdValid });
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareOrderWithNonExistingCourse()
        {
            var order = new Order { Id = OrderIdValid, UserId = UserIdValid };
            order.OrderItems.Add(new OrderItem { CourseId = CourseIdInvalid }); // non-existing course for order item

            var db = Tests.InitializeDatabase();
            await db.Orders.AddAsync(order);
            await db.Users.AddAsync(new User { Id = UserIdValid });
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareOrderWithCourseThatHasAlreadyStarted()
        {
            var currentDateTime = DateTime.UtcNow;
            var futureDateTime = currentDateTime.AddDays(1);
            var pastDateTime = currentDateTime.AddDays(-1);

            var order = new Order { Id = OrderIdValid, UserId = UserIdValid };
            order.OrderItems.Add(new OrderItem { CourseId = CourseIdFuture });
            order.OrderItems.Add(new OrderItem { CourseId = CourseIdStarted }); // course has started

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(
                new Course { Id = CourseIdStarted, StartDate = pastDateTime, Name = "Course 1", Price = 1 }, // course has started
                new Course { Id = CourseIdFuture, StartDate = futureDateTime, Name = "Course 2", Price = 2 });
            await db.Orders.AddAsync(order);
            await db.Users.AddAsync(new User { Id = UserIdValid });
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareOrderWithAllFutureCourses()
        {
            var currentDateTime = DateTime.UtcNow;
            var futureDateTime = currentDateTime.AddDays(1);

            var order = new Order { Id = OrderIdValid, UserId = UserIdValid };
            order.OrderItems.Add(new OrderItem { CourseId = CourseIdValid });
            order.OrderItems.Add(new OrderItem { CourseId = CourseIdFuture });

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(
                new Course { Id = CourseIdValid, StartDate = futureDateTime, Name = "Course 1", Price = 1 },
                new Course { Id = CourseIdFuture, StartDate = futureDateTime, Name = "Course 2", Price = 2 });
            await db.Orders.AddAsync(order);
            await db.Users.AddAsync(new User { Id = UserIdValid });
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareUserAndCoursesWithCourseThatHasAlreadyStarted()
        {
            var currentDateTime = DateTime.UtcNow;
            var futureDateTime = currentDateTime.AddDays(1);
            var pastDateTime = currentDateTime.AddDays(-1);

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(
                new Course { Id = CourseIdStarted, Name = "Course 1", Price = 1, StartDate = pastDateTime }, // course has started
                new Course { Id = CourseIdFuture, Name = "Course 2", Price = 2, StartDate = futureDateTime });
            await db.Users.AddAsync(new User { Id = UserIdValid });
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareUserAndFutureCourses()
        {
            var currentDateTime = DateTime.UtcNow;
            var futureDateTime = currentDateTime.AddDays(1);

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(
                new Course { Id = CourseIdValid, Name = "Course 1", Price = 1, StartDate = futureDateTime },
                new Course { Id = CourseIdFuture, Name = "Course 2", Price = 2, StartDate = futureDateTime });
            await db.Users.AddAsync(new User { Id = UserIdValid, Name = UserName });
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareUserWithEnrolledFutureCourses()
        {
            var currentDateTime = DateTime.UtcNow;
            var futureDateTime = currentDateTime.AddDays(1);

            var user = new User { Id = UserIdValid };
            user.Courses.Add(new StudentCourse { CourseId = CourseIdEnrolled });

            var db = Tests.InitializeDatabase();
            await db.Courses.AddRangeAsync(
                new Course { Id = CourseIdValid, Name = "Course 1", Price = 1, StartDate = futureDateTime },
                new Course { Id = CourseIdFuture, Name = "Course 2", Price = 2, StartDate = futureDateTime },
                new Course { Id = CourseIdEnrolled, Name = "Course 3", Price = 3, StartDate = futureDateTime });
            await db.Users.AddAsync(user);
            await db.SaveChangesAsync();

            return db;
        }

        private async Task<UniversityDbContext> PrepareUserOrdersWithInvoice()
        {
            var order1 = new Order
            {
                Id = OrderIdValid,
                UserId = UserIdValid,
                InvoiceId = InvoiceValid,
                OrderDate = new DateTime(2018, 1, 1),
                PaymentMethod = PaymentMethod,
                TotalPrice = 100,
                Status = Status.Pending,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Id = 1, CourseId = 100, CourseName = "Course 1", Price = 10 },
                    new OrderItem { Id = 2, CourseId = 200, CourseName = "Course 2", Price = 20 },
                }
            };
            var order2 = new Order
            {
                Id = OrderIdValid + 1,
                UserId = UserIdValid,
                InvoiceId = InvoiceValid + 1,
                OrderDate = new DateTime(2019, 8, 15),
                PaymentMethod = PaymentMethod,
                TotalPrice = 200,
                Status = Status.Completed,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { Id = 3, CourseId = 300, CourseName = "Course 3", Price = 30 },
                }
            };

            var db = Tests.InitializeDatabase();
            await db.Orders.AddRangeAsync(order1, order2);
            await db.Users.AddAsync(new User { Id = UserIdValid });
            await db.SaveChangesAsync();

            return db;
        }

        private IOrderService InitializeOrderService(UniversityDbContext db)
            => new OrderService(db, Tests.Mapper);

        private class OrderWithItems
        {
            public Order Order { get; set; }

            public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        }
    }
}
