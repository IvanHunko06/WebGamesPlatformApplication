namespace WebSocketService.Models;

public record RoomModel(string? roomId, string? roomName, string? creator, int? selectedPlayerCount, int? currentPlayersCount);