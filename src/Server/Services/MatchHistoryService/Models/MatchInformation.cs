using static System.Formats.Asn1.AsnWriter;

namespace MatchHistoryService.Models;

public class MatchInformation
{
    public int Id { get; set; }
    public DateTimeOffset TimeBegin { get; set; }
    public DateTimeOffset TimeEnd { get; set; }
    public string FinishReason { get; set; }
    public string GameId { get; set; }
    public ICollection<Score> MatchMembers { get; set; }
}
