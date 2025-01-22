namespace ProfileService.Models;

public class LimitedProfileClientModel
{
    public string Username { get; set; }
    public bool IsPrivateProfile { get; set; }
    public string PublicName { get; set; }

    public string? SmallImageUrl { get; set; }
    public string? BigImageUrl { get; set; }
}
