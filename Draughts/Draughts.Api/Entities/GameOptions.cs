using System;

namespace Draughts.Api.Entities;

public class GameOptions
{
    public GameType GameType { get; set; }
    public CreatorSide CreatorSide { get; set; }
    public Engine Engine { get; set; }
    public TimeSpan EngineThinkingTime { get; set; }
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
    MiniMax = 0,
    Random = 1
}
