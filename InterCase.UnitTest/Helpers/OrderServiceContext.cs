using AutoMapper;
using InterCase.Common;
using InterCase.Repository.Entities;
using InterCase.Repository.Interfaces;
using InterCase.Service.Interfaces;
using InterCase.Service.Mappings;
using InterCase.Service.Services.DbServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using NSubstitute;

namespace InterCase.UnitTest.Helpers
{
    public sealed class OrderServiceContext
    {
        public const string CorrelationId = "test-correlation";
        public const int OrdersTtlSeconds = 120;
        public const int ProcessingDelaySeconds = 3;

        private static readonly Lazy<IMapper> SharedMapper = new(() => new ServiceCollection()
            .AddLogging()
            .AddAutoMapper(typeof(MappingProfile))
            .BuildServiceProvider()
            .GetRequiredService<IMapper>());

        private long _nextId = 100;

        public OrderServiceContext()
        {
            Repositories.Order.Returns(Orders);
            Repositories.User.Returns(Users);

            Orders.When(r => r.AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>()))
                .Do(call => AddedOrders.Add(call.Arg<Order>()));

            Repositories.SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(_ =>
                {
                    ApplyAuditInfo();
                    return 1;
                });

            User = new User
            {
                Id = 7,
                PublicId = Guid.CreateVersion7(),
                Username = "demo01",
                DisplayName = "Demo User",
                PasswordHash = "hash"
            };
            Users.GetByPublicIdAsync(User.PublicId, true, Arg.Any<CancellationToken>()).Returns(User);
        }

        public static IMapper Mapper => SharedMapper.Value;

        public IRepositoryManager Repositories { get; } = Substitute.For<IRepositoryManager>();
        public IOrderRepository Orders { get; } = Substitute.For<IOrderRepository>();
        public IUserRepository Users { get; } = Substitute.For<IUserRepository>();
        public ICacheService Cache { get; } = Substitute.For<ICacheService>();
        public IQueueService Queue { get; } = Substitute.For<IQueueService>();
        public FakeTimeProvider Time { get; } = new(new DateTimeOffset(2026, 11, 23, 12, 0, 0, TimeSpan.Zero));
        public List<Order> AddedOrders { get; } = [];
        public User User { get; }

        public OrderService CreateService(ICacheService? cache = null) => new(
            Repositories,
            Mapper,
            cache ?? Cache,
            Queue,
            new CorrelationContext { CorrelationId = CorrelationId },
            Options.Create(new CacheOptions { OrdersTtlSeconds = OrdersTtlSeconds }),
            Options.Create(new ProcessingOptions { DelaySeconds = ProcessingDelaySeconds }),
            Time,
            NullLogger<OrderService>.Instance);

        public Order CreateOrder(OrderStatus status = OrderStatus.Pending) => new()
        {
            Id = Interlocked.Increment(ref _nextId),
            PublicId = Guid.CreateVersion7(),
            ProductId = Guid.NewGuid(),
            Quantity = 2,
            PaymentMethod = PaymentMethod.Cash,
            Status = status,
            UserId = User.Id,
            User = User,
            CreatedAt = Time.GetUtcNow().UtcDateTime,
            ProcessedAt = status == OrderStatus.Processed ? Time.GetUtcNow().UtcDateTime : null
        };

        public Order StoreOrder(OrderStatus status = OrderStatus.Pending)
        {
            var order = CreateOrder(status);
            Orders.GetByIdAsync(order.Id, true, Arg.Any<CancellationToken>()).Returns(order);
            Orders.GetByPublicIdAsync(order.PublicId, true, Arg.Any<CancellationToken>()).Returns(order);
            return order;
        }

        private void ApplyAuditInfo()
        {
            foreach (var order in AddedOrders.Where(o => o.Id == 0))
            {
                order.Id = Interlocked.Increment(ref _nextId);
                order.CreatedAt = Time.GetUtcNow().UtcDateTime;
                if (order.PublicId == Guid.Empty)
                    order.PublicId = Guid.CreateVersion7();
            }
        }
    }
}
