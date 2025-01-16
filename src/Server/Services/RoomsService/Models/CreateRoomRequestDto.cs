namespace RoomsService.Models;

public class CreateRoomRequestDto
{
    public string RoomName {  get; set; }
    public string GameId { get; set; }
    public bool IsPrivate {  get; set; }
    public int SelectedPlayersCount {  get; set; }
}
