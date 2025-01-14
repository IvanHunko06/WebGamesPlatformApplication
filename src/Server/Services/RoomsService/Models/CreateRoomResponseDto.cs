namespace RoomsService.Models;

public class CreateRoomResponseDto
{
    public bool IsSuccess {  get; set; }
    public string? ErrorMessage {  get; set; }
    public string? RoomId {  get; set; }
    public string? AccessToken {  get; set; }
}
