namespace Draughts.Api.Entities;

public class GameOptions
{
    public GameType GameType { get; set; }
    public CreatorSide CreatorSide { get; set; }
}

public enum GameType
{
    LocalMultiplayer,
    OnlineMultiplayer
}

public enum CreatorSide
{
    Random = -1,
    Black = 0,
    White = 1
}