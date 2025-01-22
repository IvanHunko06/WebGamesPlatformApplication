using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RatingService.Clients;
using RatingService.Interfaces;
using RatingService.Models;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Authentication;

namespace RatingService.Services;

[Route("/rest/RatingService")]
[ApiController]
public class RatingServiceController : ControllerBase
{
    private readonly IRatingService ratingService;
    private readonly AuthSettings authSettings;
    private readonly UserContextService userContext;
    private readonly ProfileServiceHttpClient profileService;

    public RatingServiceController(
        IRatingService ratingService, 
        IOptions<AuthSettings> authSettings,
        UserContextService userContext,
        ProfileServiceHttpClient profileService)
    {
        this.ratingService = ratingService;
        this.authSettings = authSettings.Value;
        this.userContext = userContext;
        this.profileService = profileService;
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
    [HttpGet("GetScore/{seasonId:int}/{**targetUserId}")]
    public async Task<IActionResult> GetUserScore([FromRoute] int seasonId, [FromRoute] string targetUserId)
    {
        string? userIdClaim = userContext.GetUserId(HttpContext);
        if(string.IsNullOrEmpty(userIdClaim))return BadRequest(ErrorMessages.PreferedUsernameClaimNotFound);
        var userScore = await ratingService.GetUserScore(targetUserId, seasonId);
        if (User.IsInRole(authSettings.AdminRoleClaim))
        {
            if (userScore is null) return NotFound(ErrorMessages.NoUserScore);
            return Ok(userScore);
        }
        if(userIdClaim.ToLower() == targetUserId.ToLower())
        {
            if (userScore is null) return NotFound(ErrorMessages.NoUserScore);
            return Ok(userScore);
        }
        bool? isPrivateProfile = await profileService.UserProfileIsPrivate(targetUserId);
        if (isPrivateProfile is null)
            return StatusCode(500);

        if (!isPrivateProfile.Value)
        {
            if (userScore is null) return NotFound(ErrorMessages.NoUserScore);
            return Ok(userScore);
        }
        else
        {
            return Forbid();
        }
        
    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    [HttpGet("GetScore/{targetUserId}")]
    public async Task<IActionResult> GetLastSeasonUserScore([FromRoute] string targetUserId)
    {
        string? userIdClaim = userContext.GetUserId(HttpContext);
        if (string.IsNullOrEmpty(userIdClaim)) return BadRequest(ErrorMessages.PreferedUsernameClaimNotFound);
        var season = await ratingService.GetCurrentSeason();
        if (season is null)
            return StatusCode(500, ErrorMessages.NoCurrentSeason);
        var userScore = await ratingService.GetUserScore(targetUserId, season.SeasonId);
        if (User.IsInRole(authSettings.AdminRoleClaim))
        {
            if (userScore is null) return NotFound(ErrorMessages.NoUserScore);
            return Ok(userScore);
        }
        if (userIdClaim.ToLower() == targetUserId.ToLower())
        {
            if (userScore is null) return NotFound(ErrorMessages.NoUserScore);
            return Ok(userScore);
        }
        bool? isPrivateProfile = await profileService.UserProfileIsPrivate(targetUserId);
        if (isPrivateProfile is null)
            return StatusCode(500);

        if (!isPrivateProfile.Value)
        {
            if (userScore is null) return NotFound(ErrorMessages.NoUserScore);
            return Ok(userScore);
        }
        else
        {
            return Forbid();
        }
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
