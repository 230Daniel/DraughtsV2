using System;
using System.Threading;
using Draughts.GameLogic;

namespace Draughts.Api.Services
{
    public class RandomEngine : IEngine
    {
        public int Side { get; set; }
        
        private readonly Random _random;
    
        public RandomEngine(Random random)
        {
            _random = random;
        }

        public ((int, int), (int, int)) GetMove(Board board, CancellationToken stoppingToken)
        {
            var moveIndex = _random.Next(0, board.ValidMoves.Count);
            return board.ValidMoves[moveIndex];
        }
    }
}
