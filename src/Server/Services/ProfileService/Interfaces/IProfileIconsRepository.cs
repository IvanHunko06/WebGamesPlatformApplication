using ProfileService.Entities;

namespace ProfileService.Interfaces;
public interface IProfileIconsRepository
{
    Task AddImage(ProfileImageEntity profileImage);
    Task DeleteImage(int imageId);
    Task<List<ProfileImageEntity>> GetAll();
    Task UpdateImage(ProfileImageEntity profileImage);
}