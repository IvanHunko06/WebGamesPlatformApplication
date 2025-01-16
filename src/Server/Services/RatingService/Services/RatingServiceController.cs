using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RatingService.Interfaces;
using RatingService.Models;
using SharedApiUtils.Abstractons;

namespace RatingService.Services;

[Route("/rest/RatingService")]
[ApiController]
public class RatingServiceController : ControllerBase
{
    private readonly IRatingService ratingService;

    public RatingServiceController(IRatingService ratingService)
    {
        this.ratingService = ratingService;
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    [HttpGet("GetRatingList/{seasonId:int}")]
    public async Task<IActionResult> GetRatingList([FromRoute] int seasonId)
    {
        var ratingList = await ratingService.GetRatingList(seasonId);
        if (ratingList is null)
            return NotFound();

        return Ok(ratingList);
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    [HttpGet("GetSeasonsList")]
    public async Task<IActionResult> GetSeasonsList()
    {
        var seasonsList = await ratingService.GetAllSeasons();
        return Ok(seasonsList);
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    [HttpGet("/{seasonId:int}/{**userId}")]
    public async Task<IActionResult> GetUserScore([FromRoute] int seasonId, [FromRoute] string userId)
    {
        var userScore = await ratingService.GetUserScore(userId, seasonId);
        if (userScore is null) return NotFound(ErrorMessages.NoUserScore);
        return Ok(userScore);
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    [HttpGet("SetLastSeasonUserScore")]
    public async Task<IActionResult> SetLastSeasonUserScore([FromBody] UserScoreModel userScore)
    {
        var season = await ratingService.GetCurrentSeason();
        if (season is null)
            return StatusCode(500, ErrorMessages.NoCurrentSeason);

        string? errorMessage = await ratingService.AddOrUpdateUserScore(season.SeasonId, userScore.UserId, userScore.Score);
        if (string.IsNullOrEmpty(errorMessage))
            return NoContent();
        else
            return BadRequest(errorMessage);
    }
}
