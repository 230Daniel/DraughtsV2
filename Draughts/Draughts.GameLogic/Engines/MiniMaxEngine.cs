using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Draughts.GameLogic.Engines;

public class MiniMaxEngine : IEngine
{
    public int Side { get; set; }

    private readonly ILogger<MiniMaxEngine> _logger;
    private readonly Random _random;
    private readonly Queue<Move> _moveQueue;

    public MiniMaxEngine(ILogger<MiniMaxEngine> logger, Random random)
    {
        _logger = logger;
        _random = random;
        _moveQueue = new();
    }

    public Move GetMove(Board board, CancellationToken stoppingToken)
    {
        if (_moveQueue.TryDequeue(out var queuedMove))
            return queuedMove;

        var turns = GetValidTurns(board, Side, null);
        Turn bestTurn;

        if (turns.Count == 1)
        {
            bestTurn = turns[0];
        }
        else
        {
            var bestScore = 0;
            List<Turn> bestTurns = new();

            var sw = Stopwatch.StartNew();

            var depth = 0;
            do
            {
                depth += 2;
                try
                {
                    foreach (var turn in turns)
                        turn.Score = MiniMax(turn, depth, int.MinValue, int.MaxValue, false, stoppingToken);

                    bestScore = turns.Max(x => x.Score);
                    bestTurns = turns.Where(x => x.Score == bestScore).ToList();

                    if (depth == 16) break;
                    turns.Sort((a, b) => -(a.Score.CompareTo(b.Score)));
                }
                catch (OperationCanceledException)
                {
                    depth -= 2;
                }
            } while (!stoppingToken.IsCancellationRequested);

            sw.Stop();
            _logger.LogInformation("Finished searching in {StopwatchElapsed}ms. Depth achieved: {Depth}, Best score: {BestScore}, Moves with this score: {BestMovesCount}", sw.ElapsedMilliseconds, depth, bestScore, bestTurns.Count);

            bestTurn = bestTurns[_random.Next(0, bestTurns.Count)];
        }

        foreach (var move in bestTurn.Moves.Skip(1))
            _moveQueue.Enqueue(move);

        return bestTurn.Moves[0];
    }

    private List<Turn> GetValidTurns(Board board, int side, Turn parentTurn)
    {
        if (board.NextPlayer != side)
        {
            parentTurn.BoardAfterTurn = board;
            return new() { parentTurn };
        }

        var turns = new List<Turn>();

        foreach (var move in board.ValidMoves)
        {
            var newBoard = board.Clone();
            newBoard.TakeMove(move);

            var turn = parentTurn?.CloneAndAddMove(move) ?? new Turn(move);
            var childTurns = GetValidTurns(newBoard, side, turn);

            turns.AddRange(childTurns);
        }

        return turns;
    }

    private int MiniMax(Turn turn, int depth, int alpha, int beta, bool maximising, CancellationToken stoppingToken)
    {
        var board = turn.BoardAfterTurn;

        if (depth == 0 || board.Winner != -1)
        {
            return RateBoard(board, depth);
        }

        if (maximising)
        {
            var score = int.MinValue;
            var opponentMoves = GetValidTurns(board, board.NextPlayer, null);
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
            var opponentMoves = GetValidTurns(board, board.NextPlayer, null);
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

    private int RateBoard(Board board, int depth)
    {
        if (board.Winner == Side) return int.MaxValue - 16 + depth;
        if (board.Winner == 1 - Side) return int.MinValue + 16 - depth;

        var pieceScore = 0;
        var advancementScore = 0;

        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                var tile = board.Tiles[y][x];
                if (tile == -1) continue;

                var advancement = tile > 1 ?
                    7
                    : tile % 2 == 0 ? y
                        : 7 - y;

                if (advancement == 0) advancement = 7;

                if (board.Tiles[y][x] % 2 == Side)
                {
                    pieceScore++;
                    advancementScore += advancement;
                }
                else
                {
                    pieceScore--;
                    advancementScore -= advancement;
                }
            }
        }

        return pieceScore * 10 + advancementScore;
    }

    private class Turn
    {
        public List<Move> Moves { get; init; }

        public Board BoardAfterTurn { get; set; }

        public int Score { get; set; }

        public Turn(Move move)
        {
            Moves = new List<Move> { move };
        }

        private Turn() { }

        public Turn CloneAndAddMove(Move move)
        {
            return new Turn
            {
                Moves = new List<Move>(Moves)
                {
                    move
                }
            };
        }
    }
}
