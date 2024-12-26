using StackExchange.Redis;

namespace GameSessionService;


public class RedisHelper
{
    private readonly IConfiguration configuration;
    private readonly ILogger<RedisHelper> logger;
    private readonly IDatabase redis;
    public RedisHelper(IConfiguration configuration, ILogger<RedisHelper> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
        string redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException("Redis connection string is null");
        var connection = ConnectionMultiplexer.Connect(redisConnectionString);
        redis = connection.GetDatabase();
        logger.LogInformation($"Connection established with redis via host {redisConnectionString}");
    }
    public IDatabase GetRedisDatabase()
    {
        return redis;
    }
}
