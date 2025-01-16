using MatchHistoryService.Entities;
using MatchHistoryService.Interfaces;
using MatchHistoryService.Models;
using MatchHistoryService.Repositories;
using SharedApiUtils.Abstractons;

namespace MatchHistoryService.Services;

public class MatchHistoryService : IMatchHistoryService
{
    private readonly IMatchInfoRepository matchInfoRepository;
    private readonly ILogger<MatchHistoryService> logger;

    public MatchHistoryService(IMatchInfoRepository matchInfoRepository, ILogger<MatchHistoryService> logger)
    {
        this.matchInfoRepository = matchInfoRepository;
        this.logger = logger;
    }
    public async Task<string?> AddMatchInfo(MatchInfoModel matchInfo)
    {
        try
        {
            MatchInformationEntity matchInformationEntity = new MatchInformationEntity()
            {
                FinishReason = matchInfo.FinishReason,
                GameId = matchInfo.GameId,
                RecordId = matchInfo.RecordId,
                TimeBegin = matchInfo.TimeBegin,
                TimeEnd = matchInfo.TimeEnd,
                UserScores = matchInfo.UserScoreDelta.Select(s =>
                {
                    return new UserScoreEntity()
                    {
                        ScoreDelta = s.Value,
                        UserId = s.Key,
                    };
                }).ToList()
            };
            await matchInfoRepository.AddMatchInfo(matchInformationEntity);
            logger.LogInformation($"New match info has added");
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding match information");
            return ErrorMessages.InternalServerError;
        }
    }
    public async Task<string?> DeleteMatchInfo(string recordId)
    {
        logger.LogInformation($"Deleting match {recordId} information");
        try
        {
            Guid guid = Guid.Parse(recordId);
            await matchInfoRepository.DeleteMatchInfo(guid);
            logger.LogInformation($"Match information {recordId} has deleted");
            return null;
        }
        catch (FormatException)
        {
            logger.LogWarning($"Invalid Guid format {recordId}");
            return ErrorMessages.InvalidDateFormat;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting match information");
            return ErrorMessages.InternalServerError;
        }
    }
    public async Task<List<MatchInfoModel>?> GetAllRecords()
    {
        try
        {
            List<MatchInfoModel> values = (await matchInfoRepository.GetAll()).Select(info =>
            {
                MatchInfoModel matchInfo = new MatchInfoModel()
                {
                    GameId = info.GameId,
                    FinishReason = info.FinishReason,
                    RecordId = info.RecordId,
                    TimeBegin = info.TimeBegin,
                    TimeEnd = info.TimeEnd,
                };    
                matchInfo.UserScoreDelta = info.UserScores
                .Select(x => new KeyValuePair<string, int>(x.UserId, x.ScoreDelta))
                .ToDictionary();
                return matchInfo;
            }).ToList();

            return values;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the list of all records.");
            return null;
        }
    }
    public async Task<List<MatchInfoModel>?> GetAllMatchesForUser(string userId)
    {
        try
        {
            var values = (await matchInfoRepository.GetAllForUser(userId)).Select(info =>
            {
                MatchInfoModel matchInfo = new MatchInfoModel()
                {
                    GameId = info.GameId,
                    FinishReason = info.FinishReason,
                    RecordId = info.RecordId,
                    TimeBegin = info.TimeBegin,
                    TimeEnd = info.TimeEnd,
                };
                matchInfo.UserScoreDelta = info.UserScores
                .Select(x => new KeyValuePair<string, int>(x.UserId, x.ScoreDelta))
                .ToDictionary();
                return matchInfo;
            }).ToList();
            return values;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the list of records for the user.");
            return null;
        }
    }
}
