namespace ProfileService.Entities;

public class ProfileEntity
{
    public int Id { get; set; }

    public string Username {  get; set; }
    public string PublicName { get; set; }
    public bool IsPrivateProfile { get; set; }
    public DateOnly? DOB { get; set; }

    public int? ImageId {  get; set; }
    public ProfileImageEntity? ProfileImage { get; set; }
}
