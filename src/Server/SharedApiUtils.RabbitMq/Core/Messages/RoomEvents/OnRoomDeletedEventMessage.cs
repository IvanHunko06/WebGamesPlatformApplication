﻿namespace SharedApiUtils.RabbitMq.Core.Messages.RoomEvents;

public class OnRoomDeletedEventMessage
{
    public string GameId { get; set; }
    public string RoomName { get; set; }
    public bool IsPrivate { get; set; }
    public int SelectedPlayersCount { get; set; }
    public int CurrentPlayersCount { get; set; }
    public string RoomId { get; set; }
    public string Creator { get; set; }
    public List<string> Members { get; set; }
}
