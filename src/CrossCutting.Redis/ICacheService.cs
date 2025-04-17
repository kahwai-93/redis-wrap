namespace CrossCutting.Redis
{
    public interface ICacheService
    {
        string? GetString(string key);
        bool SetString(string key, string value);
        bool SetString(string key, string value, TimeSpan expirationTime);
        bool SetString(string key, string value, DateTimeOffset expirationTime);

        T GetData<T>(string key);
        bool SetData<T>(string key, T value);
        bool SetData<T>(string key, T value, TimeSpan expirationTime);
        bool SetData<T>(string key, T value, DateTimeOffset expirationTime);

        bool RemoveData(string key);
    }
}
