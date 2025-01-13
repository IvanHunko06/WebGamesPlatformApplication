namespace WebSocketService.Models;

public record RoomClientModel(string? roomId, string? roomName, string? creator, int? selectedPlayerCount, int? currentPlayersCount);