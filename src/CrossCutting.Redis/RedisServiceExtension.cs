using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace CrossCutting.Redis
{
    public static class RedisServiceExtension
    {
        public static IServiceCollection AddRedisService(this IServiceCollection services, Action<RedisOptions> configureOptions)
        {
            services.Configure(configureOptions);

            var redisOptions = new RedisOptions();
            configureOptions?.Invoke(redisOptions);

            if (string.IsNullOrWhiteSpace(redisOptions.ConnectionString))
            {
                throw new ArgumentNullException(nameof(redisOptions.ConnectionString));
            }

            services.AddSingleton<IConnectionMultiplexer>(s => ConnectionMultiplexer.Connect(redisOptions.ConnectionString));
            services.AddScoped<IRedisCacheService, RedisCacheService>();

            return services;
        }
    }
}
