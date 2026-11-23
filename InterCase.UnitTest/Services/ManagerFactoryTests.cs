using InterCase.Common;
using InterCase.Service.Factories;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace InterCase.UnitTest.Services
{
    [TestClass]
    public sealed class ManagerFactoryTests
    {
        [TestMethod]
        public void CacheManagerFactory_ReturnsTheManagerRegisteredForTheConfiguredProvider()
        {
            var redis = Substitute.For<ICacheManager>();
            using var services = new ServiceCollection()
                .AddKeyedSingleton(CacheProvider.Redis, redis)
                .Configure<CacheOptions>(o => o.Provider = CacheProvider.Redis)
                .BuildServiceProvider();

            Assert.AreSame(redis, CacheManagerFactory.Create(services));
        }

        [TestMethod]
        public void CacheManagerFactory_WhenNoManagerIsRegisteredForTheProvider_Throws()
        {
            using var services = new ServiceCollection()
                .AddKeyedSingleton(CacheProvider.Redis, Substitute.For<ICacheManager>())
                .Configure<CacheOptions>(_ => { })
                .BuildServiceProvider();

            Assert.ThrowsExactly<InvalidOperationException>(() => CacheManagerFactory.Create(services));
        }

        [TestMethod]
        public void QueueManagerFactory_ReturnsTheManagerRegisteredForTheConfiguredProvider()
        {
            var rabbit = Substitute.For<IQueueManager>();
            using var services = new ServiceCollection()
                .AddKeyedSingleton(QueueProvider.RabbitMq, rabbit)
                .Configure<QueueOptions>(o => o.Provider = QueueProvider.RabbitMq)
                .BuildServiceProvider();

            Assert.AreSame(rabbit, QueueManagerFactory.Create(services));
        }

        [TestMethod]
        public void QueueManagerFactory_WhenNoManagerIsRegisteredForTheProvider_Throws()
        {
            using var services = new ServiceCollection()
                .AddKeyedSingleton(QueueProvider.RabbitMq, Substitute.For<IQueueManager>())
                .Configure<QueueOptions>(_ => { })
                .BuildServiceProvider();

            Assert.ThrowsExactly<InvalidOperationException>(() => QueueManagerFactory.Create(services));
        }
    }
}
