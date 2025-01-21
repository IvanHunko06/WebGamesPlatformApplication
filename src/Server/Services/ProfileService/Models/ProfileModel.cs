namespace ProfileService.Models;

public class ProfileModel
{
    public string Username {  get; set; }
    public bool IsPrivateProfile {  get; set; }
    public string PublicName { get; set; }
    
    public string? SmallImageUrl {  get; set; }
    public string? BigImageUrl {  get; set; }


    public DateOnly? DOB { get; set; }
}
