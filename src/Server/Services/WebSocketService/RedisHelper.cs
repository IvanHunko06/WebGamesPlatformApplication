using StackExchange.Redis;

namespace WebSocketService;


public class RedisHelper
{
    private readonly IConfiguration configuration;
    private readonly ILogger<RedisHelper> logger;
    private readonly IDatabase redis;
    private readonly IServer redisServer;
    public RedisHelper(IConfiguration configuration, ILogger<RedisHelper> logger)
    {
        this.configuration = configuration;
        this.logger = logger;
        string redisConnectionString = configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException("Redis connection string is null");
        var connection = ConnectionMultiplexer.Connect(redisConnectionString);
        redis = connection.GetDatabase();
        redisServer = connection.GetServer(connection.GetEndPoints().First());
        logger.LogInformation($"Connection established with redis via host {redisConnectionString}");
    }
    public IDatabase GetRedisDatabase()
    {
        return redis;
    }
    public IServer GetRedisServer()
    {
        return redisServer;
    }

}
