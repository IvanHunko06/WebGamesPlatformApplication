using Microsoft.EntityFrameworkCore;
using ProfileService.Entities;
using ProfileService.Interfaces;

namespace ProfileService.Repositories;

public class SqlServerProfileIconsRepository : IProfileIconsRepository
{
    private readonly ProfileServiceDbContext dbContext;

    public SqlServerProfileIconsRepository(ProfileServiceDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<ProfileImageEntity>> GetAll()
    {
        try
        {
            var images = await dbContext
            .ProfileImages
            .AsNoTracking()
            .ToListAsync();
            return images;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task AddImage(ProfileImageEntity profileImage)
    {
        try
        {
            dbContext.ProfileImages.Add(profileImage);
            await dbContext.SaveChangesAsync();

        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task DeleteImage(int imageId)
    {
        try
        {
            await dbContext
                .ProfileImages
                .Where(x => x.Id == imageId)
                .ExecuteDeleteAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task UpdateImage(ProfileImageEntity profileImage)
    {
        try
        {
            await dbContext
            .ProfileImages
            .Where(x => x.Id == profileImage.Id)
            .ExecuteUpdateAsync(x => x
            .SetProperty(p => p.SmallImageUrl, profileImage.SmallImageUrl)
            .SetProperty(p => p.BigImageUrl, profileImage.BigImageUrl));
        }
        catch (Exception)
        {
            throw;
        }
    }
}
