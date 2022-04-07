using System;
using System.Threading;

namespace Draughts.GameLogic.Engines;

public class RandomEngine : IEngine
{
    public int Side { get; set; }

    private readonly Random _random;

    public RandomEngine(Random random)
    {
        _random = random;
    }

    public Move GetMove(Board board, CancellationToken stoppingToken)
    {
        // When asked to get a move, choose a random move from the valid moves.
        var moveIndex = _random.Next(0, board.ValidMoves.Count);
        return board.ValidMoves[moveIndex];
    }
}
