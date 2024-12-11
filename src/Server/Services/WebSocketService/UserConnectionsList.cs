using System.Collections.Concurrent;

namespace WebSocketService;

public class UserConnectionsList
{
    private readonly ConcurrentDictionary<string, List<string>> UserConnections;
    public UserConnectionsList()
    {
        UserConnections = new ConcurrentDictionary<string, List<string>>();
    }
    public void AddUserConnectionId(string userId, string connectionId)
    {
        if (UserConnections.TryGetValue(userId, out var list))
        {
            list.Add(connectionId);
        }
        else
        {
            var list2 = new List<string>
            {
                connectionId
            };
            UserConnections.TryAdd(userId, list2);
        }
    }
    public void RemoveUserConnectionId(string userId, string connectionId)
    {
        if (UserConnections.TryGetValue(userId, out var list))
        {
            list.Remove(connectionId);
            if(list.Count == 0)
            {
                UserConnections.TryRemove(userId, out var list2);
            }
        }
    }
    public List<string> GetUserConnections(string userId)
    {
        UserConnections.TryGetValue(userId, out var list);
        return list ?? new List<string>();
    }
}
