namespace Draughts.Api.Models;

public class CreateGameModel
{
    public int GameType { get; set; }
    public int Side { get; set; }
    public int Engine { get; set; }
    public int EngineThinkingTime { get; set; }
}
