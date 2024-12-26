
using RoomsService.Repositories;

namespace RoomsService.Services;

public class RoomCleanupService : BackgroundService
{
    private readonly ILogger<RoomCleanupService> logger;
    private readonly RoomEventNotifier roomEventNotifier;
    private readonly RedisRoomRepository roomRepository;

    public RoomCleanupService(ILogger<RoomCleanupService> logger, RoomEventNotifier roomEventNotifier, RedisRoomRepository roomRepository)
    {
        this.logger = logger;
        this.roomEventNotifier = roomEventNotifier;
        this.roomRepository = roomRepository;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var roomIds = await roomRepository.GetRoomsIdsList();
            foreach(var roomId in roomIds)
            {
                var room = await roomRepository.GetRoomById(roomId);
                if (room is null) continue;
                if(room.Members.Count == 0 && room.LastModified.AddMinutes(2) < DateTimeOffset.UtcNow)
                {
                    logger.LogInformation($"Deleting empty room: {room.RoomId}");
                    await roomRepository.DeleteRoom(roomId);
                    await roomEventNotifier.NotifyRoomDeleted(room);
                }
                if(room.LastModified.AddHours(12) < DateTimeOffset.UtcNow)
                {
                    logger.LogInformation($"Deleting room after 12 hours: {room.RoomId}");
                    await roomRepository.DeleteRoom(roomId);
                    await roomEventNotifier.NotifyRoomDeleted(room);
                }
            }
        }
    }
}
