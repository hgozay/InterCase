using InterCase.Common;
using InterCase.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace InterCase.Service.Factories
{
    public static class CacheManagerFactory
    {
        public static ICacheManager Create(IServiceProvider services)
        {
            var provider = services.GetRequiredService<IOptions<CacheOptions>>().Value.Provider;
            return services.GetRequiredKeyedService<ICacheManager>(provider);
        }
    }
}
