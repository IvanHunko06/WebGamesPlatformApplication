using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ProfileService.Interfaces;
using ProfileService.Models;
using SharedApiUtils.Abstractons;
using SharedApiUtils.Abstractons.Authentication;

namespace ProfileService.Controllers;

[Route("/rest/ProfileService")]
[ApiController]
public class ProfileServiceController : ControllerBase
{
    private readonly IProfileService profileService;
    private readonly UserContextService userContextService;
    private readonly AuthSettings authSettings;

    public ProfileServiceController(
        IProfileService profileService,
        IOptions<AuthSettings> authSettings,
        UserContextService userContextService)
    {
        this.profileService = profileService;
        this.userContextService = userContextService;
        this.authSettings = authSettings.Value;
    }

    [Authorize(Policy = "OnlyPublicClient")]
    [HttpGet("GetProfile/{username}")]
    public async Task<IActionResult> GetProfile([FromRoute] string username)
    {
        var profile = await profileService.GetProfile(username);
        if (profile is null)
            return NotFound();

        if (User.IsInRole(authSettings.AdminRoleClaim))
            return Ok(profile);

        string? userId = userContextService.GetUserId(HttpContext);
        if (string.IsNullOrEmpty(userId))
            return BadRequest(ErrorMessages.PreferedUsernameClaimNotFound);

        if (userId == profile.Username)
            return Ok(profile);

        if (!profile.IsPrivateProfile)
            return Ok(profile);

        return Ok(new LimitedProfileClientModel()
        {
            Username = profile.Username,
            IsPrivateProfile = profile.IsPrivateProfile,
            PublicName = profile.PublicName,
            BigImageUrl = profile.BigImageUrl,
            SmallImageUrl = profile.SmallImageUrl,
        });

    }

    [Authorize(Policy = "OnlyPublicClient")]
    [HttpPost("GetProfiles")]
    public async Task<IActionResult> GetProfiles([FromBody] GetProfilesRequestDto request)
    {
        var profiles = await profileService.GetProfiles(request.Usernames);
        if (profiles is null)
            return NotFound();

        if (User.IsInRole(authSettings.AdminRoleClaim))
            return Ok(profiles);

        string? userId = userContextService.GetUserId(HttpContext);
        if (string.IsNullOrEmpty(userId))
            return BadRequest(ErrorMessages.PreferedUsernameClaimNotFound);

        List<object> responseProfiles = new List<object>();
        foreach (var profile in profiles)
        {
            if (profile.Username == userId)
            {
                responseProfiles.Add(profile);
                continue;
            }
            if (!profile.IsPrivateProfile)
            {
                responseProfiles.Add(profile);
                continue;
            }
            responseProfiles.Add(new LimitedProfileClientModel()
            {
                Username = profile.Username,
                IsPrivateProfile = profile.IsPrivateProfile,
                PublicName = profile.PublicName,
                BigImageUrl = profile.BigImageUrl,
                SmallImageUrl = profile.SmallImageUrl,
            });

        }

        return Ok();

    }

    [Authorize(Policy = "AllAuthenticatedUsers")]
    [HttpGet("profile/{username}/privacy")]
    public async Task<IActionResult> GetUserPrivacy(string username)
    {
        var profile = await profileService.GetProfile(username);
        if (profile is null) return NotFound();
        return Ok(new
        {
            isPrivate = profile.IsPrivateProfile,
        });
    }
    [Authorize(Policy = "AllAuthenticatedUsers")]
    [HttpGet("GetProfileIcons")]
    public async Task<IActionResult> GetProfileIcons()
    {
        var icons = await profileService.GetProfileIcons();
        return Ok(icons);
    }

    [Authorize(Policy = "OnlyPublicClient")]
    [HttpPatch("profile/{username}/privacy")]
    public async Task<IActionResult> SetUserPrivacy([FromBody] SetUserPrivacyRequestDto request, [FromRoute] string username)
    {
        var profile = await profileService.GetProfile(username);
        if (profile is null) return NotFound();

        string? userId = userContextService.GetUserId(HttpContext);
        if (string.IsNullOrEmpty(userId))
            return BadRequest(ErrorMessages.PreferedUsernameClaimNotFound);

        if (!User.IsInRole(authSettings.AdminRoleClaim) && profile.Username != userId)
            return Forbid();

        await profileService.UpdateProfilePrivacy(username, request.IsPrivateProfile);
        return NoContent();


    }


    [Authorize(Policy = "OnlyPublicClient")]
    [HttpPatch("profile/{username}/icon")]
    public async Task<IActionResult> SetProfileIcon([FromBody] SetUserIconRequestDto request, [FromRoute] string username)
    {
        var profile = await profileService.GetProfile(username);
        if (profile is null) return NotFound();

        string? userId = userContextService.GetUserId(HttpContext);
        if (string.IsNullOrEmpty(userId))
            return BadRequest(ErrorMessages.PreferedUsernameClaimNotFound);

        if (!User.IsInRole(authSettings.AdminRoleClaim) && profile.Username != userId)
            return Forbid();

        string? errorMessage = await profileService.UpdateProfileIcon(username, request.IconId);
        if (errorMessage == ErrorMessages.InternalServerError) return StatusCode(500);
        else if (!string.IsNullOrEmpty(errorMessage)) return BadRequest(errorMessage);

        return NoContent();
    }


    [Authorize(Policy = "OnlyPublicClient")]
    [HttpPut("profile/{username}/update")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request, [FromRoute] string username)
    {
        var profile = await profileService.GetProfile(username);
        if (profile is null) return NotFound();

        string? userId = userContextService.GetUserId(HttpContext);
        if (string.IsNullOrEmpty(userId))
            return BadRequest(ErrorMessages.PreferedUsernameClaimNotFound);

        if (!User.IsInRole(authSettings.AdminRoleClaim) && profile.Username != userId)
            return Forbid();

        var isSuccess = await profileService.UpdateProfile(username, request);
        if (isSuccess) return NoContent();
        else return StatusCode(500);
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    [HttpPost("AddProfileIcon")]
    public async Task<IActionResult> AddProfileIcon([FromBody] AddProfileIconRequestDto request )
    {
        bool isCreated = await profileService.AddProfileIcon(new ProfileIconModel()
        {
            BigImageUrl = request.BigImageUrl,
            SmallImageUrl = request.SmallImageUrl,
        });
        if (isCreated) return StatusCode(201);
        else return StatusCode(500);
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    [HttpPatch("UpdateProfileIcon")]
    public async Task<IActionResult> UpdateProfileIcon([FromBody] UpdateProfileIconRequestDto request)
    {
        await profileService.UpdateProfileIcon(new ProfileIconModel()
        {
            BigImageUrl = request.BigImageUrl,
            SmallImageUrl = request.SmallImageUrl,
            IconId = request.IconId,
        });
        return NoContent();
    }

    [Authorize(Policy = "AdminOrPrivateClient")]
    [HttpDelete("DeleteProfileIcon/{iconId:int}")]
    public async Task<IActionResult> DeleteProfileIcon([FromRoute] int iconId)
    {
        await profileService.DeleteProfileIcon(iconId);
        return NoContent();
    }
}
