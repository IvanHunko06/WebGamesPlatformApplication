using RatingService.Interfaces;

namespace RatingService.Services;

public class SeasonAddService : BackgroundService
{
    private IRatingService ratingService;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<SeasonAddService> logger;

    public SeasonAddService(IServiceProvider serviceProvider, ILogger<SeasonAddService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using(var scope = serviceProvider.CreateScope())
        {
            ratingService = serviceProvider.GetRequiredService<IRatingService>();
            while (!stoppingToken.IsCancellationRequested)
            {
                await TryAddSeasonForCurrentMonth();
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }
        
    }

    private async Task TryAddSeasonForCurrentMonth()
    {
        DateTime now = DateTime.Now;
        logger.LogInformation($"Trying to add a new season to the current date {now.ToShortDateString()}");
        DateOnly startDate = new DateOnly(now.Year, now.Month, 1);
        DateOnly endDate = new DateOnly(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month));
        var currentSeason = ratingService.GetCurrentSeason();
        if (currentSeason is not null)
        {
            logger.LogWarning($"Season for current date {now.ToShortDateString()} exists. Skipping adding");
            return;
        }


        string? errorMessage = await ratingService.AddSeason(startDate, endDate);
        if (string.IsNullOrEmpty(errorMessage))
            logger.LogInformation($"New season created. Start date: {startDate.ToShortDateString()} . End date: {endDate.ToShortDateString()}");
        else
            logger.LogError($"An error occurred while automatically adding the season. ErrorMessage: {errorMessage}");
    }
}
