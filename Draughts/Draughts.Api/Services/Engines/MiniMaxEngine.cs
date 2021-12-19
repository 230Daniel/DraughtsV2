using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Draughts.GameLogic;
using Microsoft.Extensions.Logging;

namespace Draughts.Api.Services
{
    public class MiniMaxEngine : IEngine
    {
        public int Side { get; set; }
        
        private readonly ILogger<MiniMaxEngine> _logger;
        private readonly Random _random;
        private readonly Queue<((int, int), (int, int))> _moveQueue;

        public MiniMaxEngine(ILogger<MiniMaxEngine> logger, Random random)
        {
            _logger = logger;
            _random = random;
            _moveQueue = new();
        }
    
        public ((int, int), (int, int)) GetMove(Board board, CancellationToken stoppingToken)
        {
            if (_moveQueue.TryDequeue(out var queuedMove))
                return queuedMove;

            _logger.LogInformation("Searching for the best move...");
            var sw = Stopwatch.StartNew();
            
            var moves = GetFullMoves(board, Side, null);
            var bestScore = 0;
            List<Move> bestMoves = new();

            var depth = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                depth += 2;
                try
                {
                    var moveScores = new Dictionary<Move, int>();
                    foreach (var move in moves)
                        moveScores[move] = MiniMax(move, depth, int.MinValue, int.MaxValue, false, stoppingToken);

                    bestScore = moveScores.Max(x => x.Value);
                    bestMoves = moveScores.Where(x => x.Value == bestScore).Select(x => x.Key).ToList();

                    if (depth == 100) break;
                    moves.Sort((a, b) => moveScores[a].CompareTo(moveScores[b]));
                }
                catch (OperationCanceledException)
                {
                    depth -= 2;
                }
            }

            sw.Stop();
            _logger.LogInformation("Finished searching in {StopwatchElapsed}ms. Depth achieved: {Depth}, Best score: {BestScore}, Moves with this score: {BestMovesCount}", sw.ElapsedMilliseconds, depth, bestScore, bestMoves.Count);

            var bestMove = bestMoves[_random.Next(0, bestMoves.Count)];
            
            var nestedMove = bestMove.NestedMove;
            while (nestedMove is not null)
            {
                _moveQueue.Enqueue((nestedMove.Origin, nestedMove.Destination));
                nestedMove = nestedMove.NestedMove;
            }
            
            return (bestMove.Origin, bestMove.Destination);
        }

        private List<Move> GetFullMoves(Board board, int side, Move parentMove)
        {
            if (board.NextPlayer != side)
            {
                if (parentMove is null) return null;
                parentMove.BoardAfterAllNestedMovesMade = board;
                return new() { parentMove };
            }

            var fullMoves = new List<Move>();
        
            foreach (var move in board.ValidMoves.Select(x => new Move(x)))
            {
                var newBoard = board.Clone();
                newBoard.TakeMove(move.Origin, move.Destination);

                var childFullMoves = GetFullMoves(newBoard, side, move);
                
                var alternativeParentMoves = parentMove is null
                    ? childFullMoves
                    : childFullMoves.Select(childMove =>
                    {
                        var newParentMove = parentMove.Clone();
                        newParentMove.NestedMove = childMove;
                        newParentMove.BoardAfterAllNestedMovesMade = childMove.BoardAfterAllNestedMovesMade;
                        return newParentMove;
                    });

                fullMoves.AddRange(alternativeParentMoves);
            }

            return fullMoves;
        }

        private int MiniMax(Move move, int depth, int alpha, int beta, bool maximising, CancellationToken stoppingToken)
        {
            var board = move.BoardAfterAllNestedMovesMade;
            
            if (depth == 0 || board.Winner != -1)
            {
                return RateBoard(board);
            }

            if (maximising)
            {
                var score = int.MinValue;
                var opponentMoves = GetFullMoves(board, board.NextPlayer, null);
                foreach (var opponentMove in opponentMoves)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    score = Math.Max(score, MiniMax(opponentMove, depth - 1, alpha, beta, false, stoppingToken));
                    if (score >= beta) break;
                    alpha = Math.Max(alpha, score);
                }
                return score;
            }
            else
            {
                var score = int.MaxValue;
                var opponentMoves = GetFullMoves(board, board.NextPlayer, null);
                foreach (var opponentMove in opponentMoves)
                {
                    stoppingToken.ThrowIfCancellationRequested();
                    score = Math.Min(score, MiniMax(opponentMove, depth - 1, alpha, beta, true, stoppingToken));
                    if (score <= alpha) break;
                    beta = Math.Min(beta, score);
                }
                    
                return score;
            }
        }

        private int RateBoard(Board board)
        {
            if (board.Winner == Side) return int.MaxValue;
            if (board.Winner == 1 - Side) return int.MinValue;
            
            var pieceScore = 0;
            for (var y = 0; y < 8; y++)
            {
                for (var x = 0; x < 4; x++)
                {
                    var tile = board.Tiles[y][x];
                    if (tile == -1) continue;
                    
                    if (board.Tiles[y][x] % 2 == Side) pieceScore++;
                    else pieceScore--;
                }
            }
            return pieceScore;
        }

        private class Move
        {
            public (int, int) Origin { get; private init; }
            public (int, int) Destination { get; private init; }
            public Move NestedMove { get; set; }
            public Board BoardAfterAllNestedMovesMade { get; set; }

            public Move(((int, int), (int, int)) move)
            {
                Origin = move.Item1;
                Destination = move.Item2;
            }

            private Move() { }

            public Move Clone()
            {
                return new Move()
                {
                    Origin = (Origin.Item1, Origin.Item2),
                    Destination = (Destination.Item1, Destination.Item2),
                    NestedMove = NestedMove?.Clone()
                };
            }
        }
    }
}
