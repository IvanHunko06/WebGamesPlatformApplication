namespace ProfileService.Models;

public class UpdateProfileRequestDto
{
    public DateOnly? DateOfBirthday { get; set; }
    public string? PublicName {  get; set; }
}
