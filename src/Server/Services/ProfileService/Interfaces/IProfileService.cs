using ProfileService.Models;

namespace ProfileService.Interfaces;

public interface IProfileService
{
    Task CreateDefaultProfile(string username);
    Task CreateDefaultProfiles(List<string> usernames);
    Task<ProfileModel?> GetProfile(string username);
    Task<List<ProfileModel>> GetProfiles(List<string> usernames);
    Task UpdateProfilePrivacy(string username, bool isPrivateProfile);
    Task<string?> UpdateProfileIcon(string username, int iconId);


    Task<List<ProfileIconModel>> GetProfileIcons();
    Task<bool> AddProfileIcon(ProfileIconModel icon);
    Task UpdateProfileIcon(ProfileIconModel icon);
    Task DeleteProfileIcon(int iconId);
}