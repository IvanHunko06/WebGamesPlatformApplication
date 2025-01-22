using Microsoft.EntityFrameworkCore;
using ProfileService.Entities;
using ProfileService.Interfaces;

namespace ProfileService.Repositories;

public class SqlServerProfilesRepository : IProfilesRepository
{
    private readonly ProfileServiceDbContext profileServiceDbContext;

    public SqlServerProfilesRepository(ProfileServiceDbContext profileServiceDbContext)
    {
        this.profileServiceDbContext = profileServiceDbContext;
    }

    public async Task<ProfileEntity?> GetProfileByUsername(string username)
    {
        try
        {
            var profile = await profileServiceDbContext
            .Profiles
            .AsNoTracking()
            .Where(x => x.Username == username)
            .Include(x=>x.ProfileImage)
            .FirstOrDefaultAsync();

            return profile;
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task<List<ProfileEntity>> GetProfilesByUsernames(List<string> usernames)
    {
        return await profileServiceDbContext
            .Profiles
            .AsNoTracking()
            .Where(p => usernames.Contains(p.Username))
            .Include(p => p.ProfileImage)
            .ToListAsync();
    }
    public async Task AddProfile(ProfileEntity profile)
    {
        try
        {
            await profileServiceDbContext.Profiles.AddAsync(profile);
            await profileServiceDbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task AddProfiles(List<ProfileEntity> profiles)
    {
        try
        {
            await profileServiceDbContext.AddRangeAsync(profiles);
            await profileServiceDbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task DeleteProfile(string username)
    {
        try
        {
            await profileServiceDbContext.
                Profiles
                .Where(x => x.Username == username)
                .ExecuteDeleteAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task DeleteProfiles(List<string> usernames)
    {
        try
        {
            await profileServiceDbContext
            .Profiles
            .Where(x => usernames.Contains(x.Username))
            .ExecuteDeleteAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
    public async Task UpdateProfile(ProfileEntity profile)
    {
        try
        {
            await profileServiceDbContext
            .Profiles
            .Where(x => x.Username == profile.Username)
            .ExecuteUpdateAsync(u => u
            .SetProperty(p => p.PublicName, profile.PublicName)
            .SetProperty(p => p.IsPrivateProfile, profile.IsPrivateProfile)
            .SetProperty(p => p.DOB, profile.DOB)
            .SetProperty(p => p.ImageId, profile.ImageId)
            );
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<List<string>> GetExistingUsernames(List<string> usernames)
    {
        try
        {
            return await profileServiceDbContext
            .Profiles
            .AsNoTracking()
            .Where(x => usernames.Contains(x.Username))
            .Select(x => x.Username)
            .ToListAsync();
        }
        catch (Exception)
        {
            throw;
        }
    }
}
