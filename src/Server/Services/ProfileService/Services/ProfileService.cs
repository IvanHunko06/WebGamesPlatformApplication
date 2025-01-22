using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ProfileService.Entities;
using ProfileService.Interfaces;
using ProfileService.Models;
using SharedApiUtils.Abstractons;

namespace ProfileService.Services;

public class ProfileService : IProfileService
{
    private readonly IProfilesRepository profilesRepository;
    private readonly IProfileIconsRepository profileIconsRepository;
    private readonly ILogger<ProfileService> logger;
    private readonly KeycloakAdmimClient keycloakAdmimClient;

    public ProfileService(
        IProfilesRepository profilesRepository,
        IProfileIconsRepository profileIconsRepository,
        ILogger<ProfileService> logger,
        KeycloakAdmimClient keycloakAdmimClient)
    {
        this.profilesRepository = profilesRepository;
        this.profileIconsRepository = profileIconsRepository;
        this.logger = logger;
        this.keycloakAdmimClient = keycloakAdmimClient;
    }
    public async Task CreateDefaultProfile(string username)
    {
        try
        {
            var existProfile = await profilesRepository.GetProfileByUsername(username);
            if (existProfile is not null) return;
            ProfileEntity profile = new ProfileEntity()
            {
                Username = username,
                DOB = null,
                PublicName = username,
                IsPrivateProfile = false
            };
            await profilesRepository.AddProfile(profile);
            logger.LogInformation($"Profile {username} created");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"an error occurred while creating an empty profile {username}");
        }
    }
    public async Task CreateDefaultProfiles(List<string> usernames)
    {
        try
        {
            var existingUsernames = await profilesRepository
                .GetExistingUsernames(usernames);

            var newUsernames = usernames.Except(existingUsernames).ToList();

            if (newUsernames.Any())
            {
                var profiles = newUsernames.Select(x => new ProfileEntity
                {
                    DOB = null,
                    PublicName = x,
                    Username = x,
                    IsPrivateProfile = false
                }).ToList();

                await profilesRepository.AddProfiles(profiles);
                logger.LogInformation($"{profiles.Count} profiles created");
            }
            else
            {
                logger.LogInformation("No new profiles to create.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while creating profiles.");
        }
    }
    public async Task<ProfileModel?> GetProfile(string username)
    {
        try
        {
        GetProfileFromDb:
            var profileEntity = await profilesRepository.GetProfileByUsername(username);
            if (profileEntity is null)
            {
                var jsonObjectProfile = await keycloakAdmimClient.GetUser(username);
                if (jsonObjectProfile is null) return null;
                string usernameFromJson = jsonObjectProfile["username"]!.GetValue<string>();
                if (usernameFromJson != username) return null;
                await CreateDefaultProfile(username);
                goto GetProfileFromDb;
            }
            var profileModel = new ProfileModel()
            {
                Username = profileEntity.Username,
                PublicName = profileEntity.PublicName,
                DOB = profileEntity.DOB,
                IsPrivateProfile = profileEntity.IsPrivateProfile,
                BigImageUrl = profileEntity.ProfileImage is not null ? profileEntity.ProfileImage.BigImageUrl : null,
                SmallImageUrl = profileEntity.ProfileImage is not null ? profileEntity.ProfileImage.SmallImageUrl : null,
            };
            return profileModel;
        }

        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred while retrieving the profile {username}");
            return null;
        }
    }
    public async Task<List<ProfileModel>> GetProfiles(List<string> usernames)
    {
        try
        {
        GetProfilesFromDb:
            var profileEntities = await profilesRepository.GetProfilesByUsernames(usernames);

            var existingUsernames = profileEntities.Select(p => p.Username).ToHashSet();
            var missingUsernames = usernames.Except(existingUsernames).ToList();

            if (missingUsernames.Any())
            {
                foreach (var username in missingUsernames)
                {
                    var jsonObjectProfile = await keycloakAdmimClient.GetUser(username);
                    if (jsonObjectProfile != null)
                    {
                        await CreateDefaultProfile(username);
                    }
                }

                goto GetProfilesFromDb;
            }

            var profileModels = profileEntities.Select(profileEntity => new ProfileModel
            {
                Username = profileEntity.Username,
                PublicName = profileEntity.PublicName,
                DOB = profileEntity.DOB,
                IsPrivateProfile = profileEntity.IsPrivateProfile,
                BigImageUrl = profileEntity.ProfileImage?.BigImageUrl,
                SmallImageUrl = profileEntity.ProfileImage?.SmallImageUrl,
            }).ToList();

            return profileModels;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving profiles.");
            return new List<ProfileModel>();
        }
    }
    public async Task UpdateProfilePrivacy(string username, bool isPrivateProfile)
    {
        try
        {
            var profileEntity = await profilesRepository.GetProfileByUsername(username);
            if (profileEntity is null)
            {
                logger.LogWarning($"Profile {username} entity is null");
                return;
            }

            profileEntity.IsPrivateProfile = isPrivateProfile;
            await profilesRepository.UpdateProfile(profileEntity);
            string newVisibilityLog = isPrivateProfile ? "private" : "public";
            logger.LogInformation($"Profile {username} is visibility set to {newVisibilityLog}");
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "an error occurred while changing profile privacy");
        }
    }
    public async Task<string?> UpdateProfileIcon(string username, int iconId)
    {
        try
        {
            var profileEntity = await profilesRepository.GetProfileByUsername(username);
            if (profileEntity is null)
            {
                logger.LogWarning($"Profile {username} entity is null");
                return ErrorMessages.UsernameNotExist;
            }
            profileEntity.ImageId = iconId;
            await profilesRepository.UpdateProfile(profileEntity);
            logger.LogInformation($"Profile {username} icon changed to {iconId}");
            return null;
        }
        catch (SqlException ex)
        {
            if (ex.Number == 547)
            {
                return ErrorMessages.ProfileIconIdNotExist;
            }
            else
            {
                return ErrorMessages.InternalServerError;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while changing the profile icon");
            return ErrorMessages.InternalServerError;
        }
    }
    public async Task<bool> UpdateProfile(string username, UpdateProfileRequestDto requestDto)
    {
        try
        {
            var profileEntity = await profilesRepository.GetProfileByUsername(username);
            if (profileEntity is null)
            {
                logger.LogWarning($"Profile {username} is null");
                return false;
            }
            if (requestDto.DateOfBirthday is not null)
                profileEntity.DOB = requestDto.DateOfBirthday;

            if (!string.IsNullOrEmpty(requestDto.PublicName?.Trim()))
                profileEntity.PublicName = requestDto.PublicName.Trim();

            await profilesRepository.UpdateProfile(profileEntity);
            logger.LogInformation($"Profile {username} has been updated");
            return true;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while changing profile data");
            return false;
        }
    }


    public async Task<List<ProfileIconModel>> GetProfileIcons()
    {
        try
        {
            var iconsEntities = await profileIconsRepository.GetAll();
            var iconsModels = iconsEntities.Select(x => new ProfileIconModel()
            {
                BigImageUrl = x.BigImageUrl,
                SmallImageUrl = x.SmallImageUrl,
                IconId = x.Id,
            }).ToList();
            return iconsModels;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the list of profile icons");
            return new List<ProfileIconModel>();
        }
    }
    public async Task<bool> AddProfileIcon(ProfileIconModel icon)
    {
        try
        {
            var iconEntity = new ProfileImageEntity()
            {
                BigImageUrl = icon.BigImageUrl,
                SmallImageUrl = icon.SmallImageUrl,
            };
            await profileIconsRepository.AddImage(iconEntity);
            return true;
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding a new profile icon");
            return false;
        }
    }
    public async Task UpdateProfileIcon(ProfileIconModel icon)
    {
        try
        {
            var iconEntity = new ProfileImageEntity()
            {
                BigImageUrl = icon.BigImageUrl,
                SmallImageUrl = icon.SmallImageUrl,
                Id = icon.IconId,
            };
            await profileIconsRepository.UpdateImage(iconEntity);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "an error occurred while changing the profile icon database record");
        }
    }
    public async Task DeleteProfileIcon(int iconId)
    {
        try
        {
            await profileIconsRepository.DeleteImage(iconId);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting the profile icon");
        }
    }

}
