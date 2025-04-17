using StackExchange.Redis;

namespace CrossCutting.Redis
{
    public interface IRedisCacheService : ICacheService
    {
        long SetIncrement(string key, long value = 1);
        long SetDecrement(string key, long value = 1);
        bool SetKeyExpiration(string key, TimeSpan expirationTime);

        Task StreamAddAsync(string streamName, NameValueEntry[] value, int? maxLength = null);
        Task<StreamEntry[]> StreamGroupReadAsync(string streamName, string groupName, string consumerName, int? count = 1);
        Task<long> StreamAcknowledgeAsync(string streamName, string groupName, string id);
        Task<StreamPendingMessageInfo[]> StreamReadPendingAsync(string streamName, string groupName, int count = 1);
        Task<StreamEntry[]> StreamClaimAsync(string streamName, string groupName, string claimingConsumer, RedisValue[] messageIds, int minIdleTimeInMs = 300000);
    }
}
