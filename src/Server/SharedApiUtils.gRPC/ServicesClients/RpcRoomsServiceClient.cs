using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Core;
using SharedApiUtils.Abstractons.Interfaces.Clients;
using SharedApiUtils.gRPC.ServicesAccessing.Connections;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;

namespace SharedApiUtils.gRPC.ServicesClients;

public class RpcRoomsServiceClient : IRoomsServiceClient
{
    private readonly RoomsServiceConnection roomsService;

    public RpcRoomsServiceClient(RoomsServiceConnection roomsService)
    {
        this.roomsService = roomsService;
    }
    public async Task<(RoomModelDto?, string?)> GetRoom(string roomId)
    {
        try
        {
            var roomsClientCombination = await roomsService.GetClient();
            GetRoomRequest request = new()
            {
                RoomId = roomId
            };
            if (roomsClientCombination.client is null || roomsClientCombination.headers is null)
                return (null, ErrorMessages.InternalServerError);
            var getRoomReply = await roomsClientCombination.client.GetRoomAsync(request, roomsClientCombination.headers);
            if (getRoomReply is null) return (null, ErrorMessages.InternalServerError);
            if(!getRoomReply.IsSuccess)
                return (null, getRoomReply.ErrorMessage);

            return (new RoomModelDto()
            {
                Creator = getRoomReply.Room.Creator,
                SelectedPlayerCount = getRoomReply.Room.SelectedPlayersCount,
                GameId = getRoomReply.Room.GameId,
                RoomId = getRoomReply.Room.RoomId,
                RoomName = getRoomReply.Room.RoomName,
                IsPrivate = getRoomReply.Room.IsPrivate,
                Members = new List<string>(getRoomReply.Members)
            }, null);
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<string?> AddToRoom(string roomId, string userId, string accessToken)
    {
        try
        {
            var combinedClient = await roomsService.GetClient();

            AddToRoomRequest request = new()
            {
                AccessToken = accessToken,
                RoomId = roomId,
                UserId = userId
            };
            if (combinedClient.client is null || combinedClient.headers is null)
                return null;
            var addToRoomReply = await combinedClient.client.AddToRoomAsync(request, combinedClient.headers);
            if (addToRoomReply.IsSuccess)
                return null;
            else
                return addToRoomReply.ErrorMessage;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<string?> RemoveFromRoom(string roomId, string userId)
    {
        try
        {
            var combinedClient = await roomsService.GetClient();

            RemoveFromRoomRequest request = new()
            {
                RoomId = roomId,
                UserId = userId
            };
            if (combinedClient.client is null || combinedClient.headers is null)
                return null;
            var removeFromRoomReply = await combinedClient.client.RemoveFromRoomAsync(request, combinedClient.headers);
            if (removeFromRoomReply.IsSuccess)
                return null;
            else
                return removeFromRoomReply.ErrorMessage;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<string?> DeleteRoom(string roomId)
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
            if (reply.IsSuccess)
                return null;
            else
                return reply.ErrorMessage;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
