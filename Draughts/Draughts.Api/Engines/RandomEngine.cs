using System;
using Draughts.GameLogic;

namespace Draughts.Api.Engines;

public class RandomEngine : IEngine
{
    private readonly Random _random;
    
    public RandomEngine(Random random)
    {
        _random = random;
    }
    
    public ((int, int), (int, int)) GetMove(Board board, int side)
    {
        var moveIndex = _random.Next(0, board.ValidMoves.Count);
        return board.ValidMoves[moveIndex];
    }
}
