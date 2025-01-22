using MatchHistoryService.Interfaces;
using MatchHistoryService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Authentication;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;

namespace MatchHistoryService.Services;

[Route("/rest/MatchHistoryService")]
[ApiController]
public class MatchHistoryController : ControllerBase
{
    private readonly IMatchHistoryService matchHistoryService;
    private readonly UserContextService userContext;
    private readonly AuthSettings authSettings;
    public MatchHistoryController(
        IMatchHistoryService matchHistoryService,
        IOptions<AuthSettings> authSettings,
        UserContextService userContext)
    {
        this.matchHistoryService = matchHistoryService;
        this.userContext = userContext;
        this.authSettings = authSettings.Value;
    }
    [Authorize(Policy = "AdminOrPrivateClient")]
    [HttpGet("GetMatchesInfo")]
    public async Task<IActionResult> GetMatchesInfo()
    {
        var matchInfos = await matchHistoryService.GetAllRecords();
        return Ok(matchInfos);
    }
    [Authorize(Policy = "AllAuthenticatedUsers")]
    [HttpGet("GetMatchesInfo/{targetUserId}")]
    public async Task<IActionResult> GetMatchesInfoForPlayer([FromRoute] string targetUserId)
    {
        List<MatchInfoModel>? matchesForUser = null;
        if (User.IsInRole(authSettings.AdminRoleClaim))
        {
            matchesForUser = await matchHistoryService.GetAllMatchesForUser(targetUserId);
            if (matchesForUser is null)
                return NotFound();
            var fullMatchInfo = matchesForUser.Select(x => new FullMatchInfoModel()
            {
                FinishReason = x.FinishReason,
                TimeBegin = x.TimeBegin,
                TimeEnd = x.TimeEnd,
                GameId = x.GameId,
                RecordId = x.RecordId,
                UserScoreDelta = x.UserScoreDelta,
                GainedScore = x.UserScoreDelta.Where(r => r.Key == targetUserId).Select(r => r.Value).First()
            });
            return Ok(fullMatchInfo);
        }
        string? userIdClaim = userContext.GetUserId(HttpContext);
        if (string.IsNullOrEmpty(userIdClaim))
            return BadRequest(ErrorMessages.PreferedUsernameClaimNotFound);

        if (userIdClaim != targetUserId)
            return Forbid();

        matchesForUser = await matchHistoryService.GetAllMatchesForUser(targetUserId);
        if (matchesForUser is null)
            return NotFound();
        var limitedMatchesForUser = matchesForUser.Select(x => new LimitedMatchInfoModel()
        {
            FinishReason = x.FinishReason,
            GameId = x.GameId,
            TimeBegin = x.TimeBegin,
            TimeEnd = x.TimeEnd,
            GainedScore = x.UserScoreDelta.Where(r => r.Key == targetUserId).Select(r => r.Value).First()
        }).ToList();
        return Ok(limitedMatchesForUser);
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    [HttpPost("AddMatchInfo")]
    public async Task<IActionResult> AddMatchInfo(MatchInfoModel matchInfo)
    {
        string? errorMessage = await matchHistoryService.AddMatchInfo(matchInfo);
        if (string.IsNullOrEmpty(errorMessage))
            return Created();
        else if (!string.IsNullOrEmpty(errorMessage) && errorMessage != ErrorMessages.InternalServerError)
            return BadRequest(errorMessage);
        else
            return StatusCode(500);
    }
    [Authorize(Policy = "AdminOrPrivateClient")]
    [HttpDelete("DeleteMatchInfo/{recordId}")]
    public async Task<IActionResult> DeleteMatchInfo(string recordId)
    {
        var reply = new DeleteMatchInfoReply();

        string? errorMessage = await matchHistoryService.DeleteMatchInfo(recordId);
        if (string.IsNullOrEmpty(errorMessage))
            return NoContent();
        else if (!string.IsNullOrEmpty(errorMessage) && errorMessage != ErrorMessages.InternalServerError)
            return BadRequest(errorMessage);
        else
            return StatusCode(500);
    }
}
