using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RoomsService.Interfaces;
using RoomsService.Models;
using SharedApiUtils.Abstractons;

namespace RoomsService.Services;

[Route("rest/RoomsService")]
[ApiController]
public class RoomsServiceController : ControllerBase
{
    private readonly IRoomsService roomsService;
    private readonly IUserContextService userContextService;

    public RoomsServiceController(IRoomsService roomsService, IUserContextService userContextService)
    {
        this.roomsService = roomsService;
        this.userContextService = userContextService;
    }
    [Authorize(Policy = "OnlyPublicClient")]
    [HttpPost("CreateRoom")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequestDto request)
    {
        CreateRoomResponseDto reply = new CreateRoomResponseDto();
        string? userId = userContextService.GetUserId(Request.HttpContext);
        if (userId is null)
        {
            reply.ErrorMessage = ErrorMessages.PreferedUsernameClaimNotFound;
            return BadRequest(reply);
        }


        var createResult = await roomsService.CreateRoom(request.GameId, request.RoomName, userId, request.IsPrivate, request.SelectedPlayersCount);
        if (!string.IsNullOrEmpty(createResult.errorMessage) && createResult.errorMessage != ErrorMessages.InternalServerError)
        {
            reply.ErrorMessage = createResult.errorMessage;
            return BadRequest(reply);
        }
        else if (!string.IsNullOrEmpty(createResult.errorMessage) && createResult.errorMessage == ErrorMessages.InternalServerError)
        {
            return StatusCode(500);
        }
        reply.RoomId = createResult.roomId;
        reply.IsSuccess = true;
        if (!string.IsNullOrEmpty(createResult.accessToken))
            reply.AccessToken = createResult.accessToken;
        return StatusCode(201, reply);
    }

    [Authorize(Policy = "OnlyPublicClient")]
    [HttpDelete("DeleteRoom/{roomId}")]
    public async Task<IActionResult> DeleteRoom([FromRoute] string roomId)
    {
        string? userId = userContextService.GetUserId(Request.HttpContext);
        if (userId is null)
            return BadRequest(ErrorMessages.PreferedUsernameClaimNotFound);

        string? errorMessage = await roomsService.DeleteRoom(roomId, userId);
        if (string.IsNullOrEmpty(errorMessage)) return NoContent();
        else return BadRequest(errorMessage);
    }

    [Authorize(Policy = "OnlyPublicClient")]
    [HttpGet("GetPublicRoomsList/{gameId}")]
    public async Task<IActionResult> GetPublicRoomsList([FromRoute] string gameId)
    {
        var rooms = await roomsService.GetRoomsList(true, gameId);
        return Ok(rooms);
    }
}
