using RatingService.Models;

namespace RatingService.Services;

public class SeasonService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public SeasonService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            DateTime currentDate = DateTime.UtcNow;
            if (currentDate.Day == 1) 
            {
                await AddSeasonForCurrentMonth();
            }

            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }

    private async Task AddSeasonForCurrentMonth()
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<RatingDbContext>();

            DateTime now = DateTime.UtcNow;
            int daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);

            DateTime startDate = new DateTime(now.Year, now.Month, 1);
            DateTime endDate = new DateTime(now.Year, now.Month, daysInMonth); 

            var newSeason = new Season
            {
                DateStart = startDate,
                DateEnd = endDate
            };

            dbContext.Seasons.Add(newSeason);
            await dbContext.SaveChangesAsync();
        }
    }
}
