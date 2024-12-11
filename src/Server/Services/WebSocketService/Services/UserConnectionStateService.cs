using System.Collections.Concurrent;

namespace WebSocketService.Services;

public class UserConnectionStateService
{
    //private readonly ConcurrentDictionary<string, CancellationTokenSource> _userDisconnectionTokens = new();
    public event Action<string>? OnUserDisconnected;
    public event Action<string>? OnUserConnected;
    private readonly ILogger<UserConnectionStateService> logger;
    private readonly ConcurrentDictionary<string, string> userConnections;

    public UserConnectionStateService(ILogger<UserConnectionStateService> logger)
    {
        this.logger = logger;
        userConnections = new();
    }
    public void AddOrSetUserConnectionId(string userId, string connectionId)
    {
        if (userConnections.TryGetValue(userId, out var connection))
        {
            connection = connectionId;
        }
        else
        {
            userConnections.TryAdd(userId, connectionId);
            OnUserConnected?.Invoke(userId);
        }

    }
    public bool RemoveUserConnection(string userId, string connectionId)
    {
        bool isSuccess = userConnections.TryRemove(userId, out _);
        if (isSuccess)
            OnUserDisconnected?.Invoke(userId);
        return isSuccess;

    }
    public string? GetUserConnection(string userId)
    {
        if (userConnections.TryGetValue(userId, out var connection))
            return connection;
        else
            return default;

    }

    //private async Task disconnectUserTimer(string userId)
    //{
    //    logger.LogInformation($"User {userId} has disconnected");
    //    var cts = new CancellationTokenSource();
    //    if(_userDisconnectionTokens.TryAdd(userId, cts))
    //    {
    //        try
    //        {
    //            await Task.Delay(userReconnectTimeout, cts.Token);
    //            OnUserDisconnected?.Invoke(userId);
    //            logger.LogInformation($"OnUserDisconnection event for user {userId} was called");
    //        }
    //        catch (TaskCanceledException)
    //        {
    //            logger.LogInformation($"User {userId} reconnected within {userReconnectTimeout.ToString()}");
    //        }
    //        finally
    //        {
    //            _userDisconnectionTokens.TryRemove(userId, out _);
    //        }
    //    }
    //}

}
