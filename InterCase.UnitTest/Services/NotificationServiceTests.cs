using InterCase.Common;
using InterCase.Common.Dtos.Paging;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using InterCase.Service.Services.DbServices;
using InterCase.UnitTest.Helpers;
using NSubstitute;

namespace InterCase.UnitTest.Services
{
    [TestClass]
    public sealed class NotificationServiceTests
    {
        [TestMethod]
        public async Task GetByUserIdAsync_ReturnsTheRequestedPageWithPublicIds()
        {
            var userId = Guid.NewGuid();
            var order = new Order { Id = 5, PublicId = Guid.CreateVersion7(), ProductId = Guid.NewGuid() };
            var notification = new Notification
            {
                Id = 9,
                PublicId = Guid.CreateVersion7(),
                Order = order,
                OrderId = order.Id,
                Channel = NotificationChannels.Console,
                Message = "Your order has been processed.",
                SentAt = new DateTime(2026, 11, 23, 12, 0, 5, DateTimeKind.Utc)
            };
            var repositories = Substitute.For<IRepositoryManager>();
            var notifications = Substitute.For<INotificationRepository>();
            repositories.Notification.Returns(notifications);
            notifications.GetPageByUserIdAsync(userId, 3, 10, Arg.Any<CancellationToken>())
                .Returns((new List<Notification> { notification }, 21));

            var result = await new NotificationService(repositories, OrderServiceContext.Mapper)
                .GetByUserIdAsync(userId, new PageRequest { Page = 3, PageSize = 10 });

            Assert.AreEqual(3, result.Page);
            Assert.AreEqual(10, result.PageSize);
            Assert.AreEqual(21, result.TotalCount);
            Assert.AreEqual(3, result.TotalPages);
            var item = Assert.ContainsSingle(result.Items);
            Assert.AreEqual(notification.PublicId, item.Id);
            Assert.AreEqual(order.PublicId, item.OrderId);
            Assert.AreEqual(NotificationChannels.Console, item.Channel);
            Assert.AreEqual(notification.Message, item.Message);
            Assert.AreEqual(notification.SentAt, item.SentAt);
        }

        [TestMethod]
        public async Task GetByUserIdAsync_WhenUserHasNoNotifications_ReturnsEmptyPage()
        {
            var userId = Guid.NewGuid();
            var repositories = Substitute.For<IRepositoryManager>();
            var notifications = Substitute.For<INotificationRepository>();
            repositories.Notification.Returns(notifications);
            notifications.GetPageByUserIdAsync(userId, 1, 10, Arg.Any<CancellationToken>())
                .Returns((new List<Notification>(), 0));

            var result = await new NotificationService(repositories, OrderServiceContext.Mapper)
                .GetByUserIdAsync(userId, new PageRequest());

            Assert.IsEmpty(result.Items);
            Assert.AreEqual(0, result.TotalCount);
            Assert.AreEqual(0, result.TotalPages);
        }
    }
}
