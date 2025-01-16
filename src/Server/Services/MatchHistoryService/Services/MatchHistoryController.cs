using MatchHistoryService.Interfaces;
using MatchHistoryService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedApiUtils.Abstractons;
using SharedApiUtils.gRPC.ServicesAccessing.Protos;

namespace MatchHistoryService.Services;

[Route("/rest/MatchHistoryService")]
[ApiController]
public class MatchHistoryController : ControllerBase
{
    private readonly IMatchHistoryService matchHistoryService;

    public MatchHistoryController(IMatchHistoryService matchHistoryService)
    {
        this.matchHistoryService = matchHistoryService;
    }
    [Authorize(Policy = "AdminOrPrivateClient")]
    [HttpGet("GetMatchesInfo")]
    public async Task<IActionResult> GetMatchesInfo()
    {
        var matchInfos = await matchHistoryService.GetAllRecords();
        return Ok(matchInfos);
    }
    [Authorize(Policy = "AllAuthenticatedUsers")]
    [HttpGet("GetMatchesInfo/{userId}")]
    public async Task<IActionResult> GetMatchesInfoForPlayer([FromRoute] string userId)
    {
        var reply = new GetMatchesInfoForPlayerReply();

        var matchesForUser = await matchHistoryService.GetAllMatchesForUser(userId);
        if (matchesForUser is null)
            return NotFound();

        return Ok(matchesForUser);
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
