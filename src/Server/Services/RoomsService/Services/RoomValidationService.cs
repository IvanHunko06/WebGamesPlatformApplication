using SharedApiUtils.ServicesAccessing.Protos;

namespace RoomsService.Services;

public class RoomValidationService
{
    public string? ValidateRoomName(string roomName)
    {
        roomName = roomName.Trim();
        if (roomName.Length < 5) return "ROOM_NAME_LENGTH_REQUIRED_TO_BE_MORE_THAN_5";
        if (roomName.Length > 20) return "ROOM_NAME_LENGTH_REQUIRED_TO_BE_LESS_THAN_20";
        return null;
    }

    public string? ValidatePlayersCount(GameInfo gameInfo, int selectedPlayersCount)
    {
        if (!gameInfo.StaticPlayersCount)
        {
            if (selectedPlayersCount < gameInfo.MinPlayersCount || selectedPlayersCount > gameInfo.MaxPlayersCount)
            {
                return $"PLAYER_COUNT_MUST_BE_BETWEEN_{gameInfo.MinPlayersCount}_AND_{gameInfo.MaxPlayersCount}";
            }
        }
        return null;
    }
}
