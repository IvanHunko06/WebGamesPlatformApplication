using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedApiUtils.Abstractons.Core;

public class GameSessionDto
{
    public string SessionId { get; set; }
    public string OwnerId {  get; set; }
    public string RoomId {  get; set; }
    public string GameId {  get; set; }
    public List<string> Players {  get; set; }
    public Dictionary<string, int> PlayerScores { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset? EndTime {  get; set; }
	public string SessionState {  get; set; }
}
