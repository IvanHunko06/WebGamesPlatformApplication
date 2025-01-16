
using RoomsService.Interfaces;
using RoomsService.Repositories;

namespace RoomsService.Services;

public class RoomCleanupService : BackgroundService
{
    private readonly ILogger<RoomCleanupService> logger;
    private readonly IRoomsService roomsService;

    public RoomCleanupService(ILogger<RoomCleanupService> logger, IRoomsService roomsService)
    {
        this.logger = logger;
        this.roomsService = roomsService;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var rooms = await roomsService.GetRoomsList();
            foreach (var room in rooms)
            {
                if (room.Members.Count == 0 && room.LastModified.AddMinutes(2) < DateTimeOffset.UtcNow)
                {
                    logger.LogInformation($"Deleting empty room: {room.RoomId}");
                    await roomsService.DeleteRoom(room.RoomId);
                }
                if (room.LastModified.AddHours(12) < DateTimeOffset.UtcNow)
                {
                    logger.LogInformation($"Deleting room after 12 hours: {room.RoomId}");
                    await roomsService.DeleteRoom(room.RoomId);
                }
            }
        }
    }
}
