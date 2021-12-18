namespace Draughts.Api.Entities;

public class GameOptions
{
    public GameType GameType { get; set; }
    public CreatorSide CreatorSide { get; set; }
    public Engine Engine { get; set; }
}

public enum GameType
{
    LocalMultiplayer,
    OnlineMultiplayer,
    Computer
}

public enum CreatorSide
{
    Random = -1,
    Black = 0,
    White = 1
}

public enum Engine
{
    Random = 0,
    MiniMax = 1
}
