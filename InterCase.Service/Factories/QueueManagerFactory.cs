using InterCase.Common;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InterCase.Service.Factories
{
    public static class QueueManagerFactory
    {
        public static IQueueManager Create(IServiceProvider services)
        {
            var provider = services.GetRequiredService<IOptions<QueueOptions>>().Value.Provider;
            return services.GetRequiredKeyedService<IQueueManager>(provider);
        }
    }
}
