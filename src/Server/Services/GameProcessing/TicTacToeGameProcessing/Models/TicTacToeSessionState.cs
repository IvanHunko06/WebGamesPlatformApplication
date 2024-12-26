namespace TicTacToeGameProcessing.Models;

public class TicTacToeSessionState
{
    public List<char> GameBoard {  get; set; }
    public string CurrentPlayer {  get; set; }
    public string XPlayer {  get; set; }
    public string OPlayer { get; set; }

    public TicTacToeSessionState()
    {
        
    }
    public TicTacToeSessionState(TicTacToeSessionState sessionState)
    {
        GameBoard = sessionState.GameBoard;
        CurrentPlayer = sessionState.CurrentPlayer;
        XPlayer = sessionState.XPlayer;
        OPlayer = sessionState.OPlayer;
    }
}
