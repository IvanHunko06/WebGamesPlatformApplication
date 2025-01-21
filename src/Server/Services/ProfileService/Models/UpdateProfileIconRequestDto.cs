namespace ProfileService.Models;

public class UpdateProfileIconRequestDto
{
    public int IconId {  get; set; }
    public string SmallImageUrl { get; set; }
    public string BigImageUrl { get; set; }
}
