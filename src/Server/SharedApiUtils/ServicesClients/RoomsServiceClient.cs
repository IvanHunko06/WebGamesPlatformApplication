using SharedApiUtils.Interfaces;
using SharedApiUtils.ServicesAccessing.Connections;
using SharedApiUtils.ServicesAccessing.Protos;

namespace SharedApiUtils.ServicesClients;

public class RoomsServiceClient : IRoomsServiceClient
{
    private readonly RoomsServiceConnection roomsService;

    public RoomsServiceClient(RoomsServiceConnection roomsService)
    {
        this.roomsService = roomsService;
    }
    public async Task<GetRoomReply?> GetRoom(string roomId)
    {
        try
        {
            var roomsClientCombination = await roomsService.GetClient();
            GetRoomReply? getRoomReply = null;
            GetRoomRequest request = new()
            {
                RoomId = roomId
            };
            if (roomsClientCombination.client is null || roomsClientCombination.headers is null)
                return null;
            getRoomReply = await roomsClientCombination.client.GetRoomAsync(request, roomsClientCombination.headers);
            return getRoomReply;
        }
        catch (Exception)
        {
            throw;
        }
        
    }
    public async Task<AddToRoomReply?> AddToRoom(string roomId, string userId, string accessToken)
    {
        try
        {
            AddToRoomReply? addToRoomReply = null;
            var combinedClient = await roomsService.GetClient();

            AddToRoomRequest request = new()
            {
                AccessToken = accessToken,
                RoomId = roomId,
                UserId = userId
            };
            if (combinedClient.client is null || combinedClient.headers is null)
                return null;
            addToRoomReply = await combinedClient.client.AddToRoomAsync(request, combinedClient.headers);
            return addToRoomReply;
        }
        catch (Exception) 
        { 
            throw; 
        }
        
    }
    public async Task<RemoveFromRoomReply?> RemoveFromRoom(string roomId, string userId)
    {
        try
        {
            RemoveFromRoomReply? removeFromRoomReply = null;
            var combinedClient = await roomsService.GetClient();

            RemoveFromRoomRequest request = new()
            {
                RoomId = roomId,
                UserId = userId
            };
            if (combinedClient.client is null || combinedClient.headers is null)
                return null;
            removeFromRoomReply = await combinedClient.client.RemoveFromRoomAsync(request, combinedClient.headers);
            return removeFromRoomReply;
        }
        catch (Exception) 
        { 
            throw; 
        }
        
    }
    public async Task<DeleteRoomReply?> DeleteRoom(string roomId)
    {
        try
        {
            var combinedClient = await roomsService.GetClient();

            DeleteRoomRequest request = new()
            {
                RoomId = roomId
            };
            if (combinedClient.client is null || combinedClient.headers is null)
                return null;
            var reply = await combinedClient.client.DeleteRoomAsync(request, combinedClient.headers);
            return reply;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
