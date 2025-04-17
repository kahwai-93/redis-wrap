namespace CrossCutting.Redis
{
    public class RedisOptions
    {
        public int DefaultQueueSize { get; set; }
        public string ConnectionString { get; set; } = null!;
        public TimeSpan DefaultExpiredDuration { get; set; } = TimeSpan.FromSeconds(300);
    }
}
