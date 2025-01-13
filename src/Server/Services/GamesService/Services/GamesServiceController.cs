using GamesService.Interfaces;
using GamesService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GamesService.Services;

[Route("/rest/GamesService")]
[ApiController]
public class GamesServiceController : ControllerBase
{
    private readonly IGamesService gamesService;


    public GamesServiceController(IGamesService gamesService)
    {
        this.gamesService = gamesService;
    }
    [HttpPost("AddGame")]
    [Authorize(Policy = "AdminOrPrivateClient")]
    public async Task<IActionResult> AddGame([FromBody] GameInfoModel gameInfo)
    {

        string? errorMessage = await gamesService.AddGame(gameInfo);
        if (string.IsNullOrEmpty(errorMessage))
            return Created();
        else
            return BadRequest(errorMessage);
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    [HttpDelete("DeleteGame/{gameId}")]
    public async Task<IActionResult> DeleteGame([FromRoute] string gameId)
    {
        string? errorMessage = await gamesService.DeleteGame(gameId);
        if (string.IsNullOrEmpty(errorMessage))
            return NoContent();
        else
            return BadRequest(errorMessage);
    }
    [Authorize(Policy = "AdminOrPrivateClient")]
    [HttpPut("UpdateGame")]
    public async Task<IActionResult> UpdateGame([FromBody] GameInfoModel gameInfo)
    {
        string? errorMessage = await gamesService.UpdateGame(gameInfo);
        if (string.IsNullOrEmpty(errorMessage))
            return NoContent();
        else
            return BadRequest(errorMessage);
    }
    [Authorize(Policy = "AllAuthenticatedUsers")]
    [HttpGet("GetGamesList")]
    public async Task<IActionResult> GetGamesList()
    {
        var gamesList = await gamesService.GetGamesList();
        return Ok(gamesList);
    }
}
