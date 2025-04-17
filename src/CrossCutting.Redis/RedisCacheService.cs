using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;

namespace CrossCutting.Redis
{
    public class RedisCacheService : IRedisCacheService
    {
        private readonly IDatabase _db;
        private readonly RedisOptions _options;
        private readonly ILogger _logger;

        public RedisCacheService(
            IOptions<RedisOptions> redisOptions,
            IConnectionMultiplexer connectionMultiplexer,
            ILogger<RedisCacheService> logger)
        {
            _db = connectionMultiplexer.GetDatabase();
            _options = redisOptions.Value;
            _logger = logger;
        }

        public string? GetString(string key)
        {
            try
            {
                return _db.StringGet(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return default;
            }
        }

        public bool SetString(string key, string value)
            => SetData(key, value, _options.DefaultExpiredDuration);

        public bool SetString(string key, string value, TimeSpan expirationTime)
            => SetData(key, value, expirationTime);

        public bool SetString(string key, string value, DateTimeOffset expirationTime)
            => SetData(key, value, expirationTime.DateTime.Subtract(DateTime.Now));

        public T GetData<T>(string key)
        {
            try
            {
                string? value = _db.StringGet(key);

                return !string.IsNullOrEmpty(value) ? JsonSerializer.Deserialize<T>(value) : default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return default;
            }
        }

        public bool SetData<T>(string key, T value)
            => SetData(key, JsonSerializer.Serialize(value), _options.DefaultExpiredDuration);

        public bool SetData<T>(string key, T value, TimeSpan expirationTime)
            => SetData(key, JsonSerializer.Serialize(value), expirationTime);

        public bool SetData<T>(string key, T value, DateTimeOffset expirationTime)
            => SetData(key, JsonSerializer.Serialize(value), expirationTime.DateTime.Subtract(DateTime.Now));

        private bool SetData(string key, string value, TimeSpan expiryTime)
        {
            try
            {
                return _db.StringSet(key, value, expiryTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return false;
            }
        }

        [Obsolete]
        private T GetData<T>(string key, Func<T> dataSourceFunc, TimeSpan expirationTime)
        {
            //private static readonly object _lock = new();
            object _lock = new();

            var cacheData = GetData<T>(key);

            if (cacheData == null)
            {
                lock (_lock)
                {
                    cacheData = GetData<T>(key);

                    if (cacheData == null)
                    {
                        cacheData = dataSourceFunc();

                        if (cacheData != null)
                        {
                            SetData(key, cacheData, expirationTime);
                        }
                    }
                }
            }

            return cacheData;
        }

        public bool RemoveData(string key)
        {
            try
            {
                return _db.KeyExists(key) ? _db.KeyDelete(key) : false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return false;
            }
        }

        public long SetIncrement(string key, long value = 1)
        {
            try
            {
                return _db.StringIncrement(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return default;
            }
        }

        public long SetDecrement(string key, long value = 1)
        {
            try
            {
                return _db.StringDecrement(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return default;
            }
        }

        public bool SetKeyExpiration(string key, TimeSpan expirationTime)
        {
            try
            {
                return _db.KeyExpire(key, expirationTime);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                return false;
            }
        }

        public async Task StreamAddAsync(string streamName, NameValueEntry[] value, int? maxLength = null)
        {
            if (maxLength == null || maxLength == 0)
                maxLength = _options.DefaultQueueSize == 0 ? 200 : _options.DefaultQueueSize;

            await _db.StreamAddAsync(streamName, value, maxLength: maxLength, useApproximateMaxLength: true);
        }

        public async Task<StreamEntry[]> StreamGroupReadAsync(string streamName, string groupName, string consumerName, int? count = 1)
        {
            if (!(await _db.KeyExistsAsync(streamName)) || (await _db.StreamGroupInfoAsync(streamName)).All(x => x.Name != groupName))
            {
                await _db.StreamCreateConsumerGroupAsync(streamName, groupName, "0-0", true);
            }

            return await _db.StreamReadGroupAsync(streamName, groupName, consumerName, ">", count);
        }

        public async Task<long> StreamAcknowledgeAsync(string streamName, string groupName, string id)
            => await _db.StreamAcknowledgeAsync(streamName, groupName, id);

        public async Task<StreamPendingMessageInfo[]> StreamReadPendingAsync(string streamName, string groupName, int count = 1)
            => await _db.StreamPendingMessagesAsync(streamName, groupName, count, RedisValue.Null);

        public async Task<StreamEntry[]> StreamClaimAsync(string streamName, string groupName, string claimingConsumer, RedisValue[] messageIds, int minIdleTimeInMs = 300000)
        {
            return await _db.StreamClaimAsync(streamName,
                    groupName,
                    claimingConsumer: claimingConsumer,
                    minIdleTimeInMs: minIdleTimeInMs,
                    messageIds: messageIds);
        }
    }
}
