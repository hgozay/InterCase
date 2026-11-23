using InterCase.Common;
using InterCase.Common.Dtos.Notifications;
using InterCase.Common.Dtos.Orders;
using InterCase.Common.Dtos.Users;
using InterCase.Repository.Entities;
using InterCase.Service.Mappings;
using InterCase.UnitTest.Helpers;

namespace InterCase.UnitTest.Mappings
{
    [TestClass]
    public sealed class OrderMappingTests
    {
        private static Notification NotificationFor(Order order, string message, DateTime sentAt) => new()
        {
            PublicId = Guid.CreateVersion7(),
            Order = order,
            OrderId = order.Id,
            UserId = order.UserId,
            Channel = NotificationChannels.Console,
            Message = message,
            SentAt = sentAt
        };

        [TestMethod]
        public void OrderResponse_UsesPublicIdsAndTheLatestNotification()
        {
            var context = new OrderServiceContext();
            var order = context.CreateOrder(OrderStatus.Processed);
            var earlier = new DateTime(2026, 11, 23, 12, 0, 1, DateTimeKind.Utc);
            order.Notifications.Add(NotificationFor(order, "first", earlier));
            order.Notifications.Add(NotificationFor(order, "latest", earlier.AddSeconds(5)));

            var response = OrderServiceContext.Mapper.Map<OrderResponse>(order);

            Assert.AreEqual(order.PublicId, response.Id);
            Assert.AreEqual(context.User.PublicId, response.UserId);
            Assert.IsNotNull(response.Notification);
            Assert.AreEqual("latest", response.Notification.Message);
            Assert.AreEqual(NotificationChannels.Console, response.Notification.Channel);
            Assert.AreEqual(earlier.AddSeconds(5), response.Notification.SentAt);
        }

        [TestMethod]
        public void OrderResponse_WhenOrderHasNoNotification_NotificationIsNull()
        {
            var order = new OrderServiceContext().CreateOrder();

            var response = OrderServiceContext.Mapper.Map<OrderResponse>(order);

            Assert.IsNull(response.Notification);
        }

        [TestMethod]
        public void NotificationResponse_UsesPublicIdsOfNotificationAndOrder()
        {
            var order = new OrderServiceContext().CreateOrder(OrderStatus.Processed);
            var notification = NotificationFor(order, "done", DateTime.UtcNow);

            var response = OrderServiceContext.Mapper.Map<NotificationResponse>(notification);

            Assert.AreEqual(notification.PublicId, response.Id);
            Assert.AreEqual(order.PublicId, response.OrderId);
            Assert.AreEqual("done", response.Message);
        }

        [TestMethod]
        public void OrderLogResponse_LeavesTheLogForTheServiceToFill()
        {
            var order = new OrderServiceContext().CreateOrder(OrderStatus.Processed);

            var response = OrderServiceContext.Mapper.Map<OrderLogResponse>(order);

            Assert.AreEqual(order.PublicId, response.OrderId);
            Assert.AreEqual(OrderStatus.Processed, response.Status);
            Assert.AreEqual(order.ProcessedAt, response.ProcessedAt);
            Assert.IsNull(response.Log);
        }

        [TestMethod]
        public void OrderPlacedMessage_UsesDatabaseIdUserPublicIdAndCorrelationFromItems()
        {
            var context = new OrderServiceContext();
            var order = context.CreateOrder();

            var message = OrderServiceContext.Mapper.Map<OrderPlacedMessage>(order, options =>
                options.Items[MappingProfile.CorrelationIdKey] = "corr-7");

            Assert.AreEqual(order.Id, message.OrderId);
            Assert.AreEqual(context.User.PublicId, message.UserId);
            Assert.AreEqual(order.ProductId, message.ProductId);
            Assert.AreEqual("corr-7", message.CorrelationId);
        }

        [TestMethod]
        public void UserResponse_UsesPublicId()
        {
            var user = new OrderServiceContext().User;

            var response = OrderServiceContext.Mapper.Map<UserResponse>(user);

            Assert.AreEqual(user.PublicId, response.Id);
            Assert.AreEqual(user.DisplayName, response.DisplayName);
        }

        [TestMethod]
        public void UpdateOrderRequest_ChangesOnlyEditableFields()
        {
            var context = new OrderServiceContext();
            var order = context.CreateOrder();
            var originalPublicId = order.PublicId;
            var request = new UpdateOrderRequest { ProductId = Guid.NewGuid(), Quantity = 9, PaymentMethod = PaymentMethod.CreditCard };

            OrderServiceContext.Mapper.Map(request, order);

            Assert.AreEqual(request.ProductId, order.ProductId);
            Assert.AreEqual(9, order.Quantity);
            Assert.AreEqual(PaymentMethod.CreditCard, order.PaymentMethod);
            Assert.AreEqual(originalPublicId, order.PublicId);
            Assert.AreEqual(OrderStatus.Pending, order.Status);
            Assert.AreSame(context.User, order.User);
        }
    }
}
