namespace ProfileService.Entities;

public class ProfileImageEntity
{
    public int Id { get; set; }
    public string SmallImageUrl {  get; set; }
    public string BigImageUrl {  get; set; }

    public List<ProfileEntity> Profiles { get; set; }
}
