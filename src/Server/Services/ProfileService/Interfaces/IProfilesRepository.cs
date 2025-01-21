using ProfileService.Entities;

namespace ProfileService.Interfaces;

public interface IProfilesRepository
{
    Task AddProfile(ProfileEntity profile);
    Task AddProfiles(List<ProfileEntity> profiles);
    Task DeleteProfile(string username);
    Task DeleteProfiles(List<string> usernames);
    Task<ProfileEntity?> GetProfileByUsername(string username);
    Task<List<ProfileEntity>> GetProfilesByUsernames(List<string> usernames);
    Task UpdateProfile(ProfileEntity profile);
    Task<List<string>> GetExistingUsernames(List<string> usernames);
}