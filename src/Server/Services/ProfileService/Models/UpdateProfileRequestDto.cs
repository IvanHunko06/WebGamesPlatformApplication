namespace ProfileService.Models;

public class UpdateProfileRequestDto
{
    public DateOnly? DayOfBirthday { get; set; }
    public string? PublicName {  get; set; }
}
