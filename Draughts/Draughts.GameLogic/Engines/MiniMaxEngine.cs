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
        // First check if we already queued up the next move, and if we have dequeue it and return it.
        // This will happen in the event of multiple jumps on the computer's turn.
        if (_moveQueue.TryDequeue(out var queuedMove))
            return queuedMove;

        // A "turn" is a move or set of moves (in the case of multiple jumps) that fully describe what a player does on their turn
        // The MiniMax algorithm can not cope with one player making multiple moves in sequence (in the case of multiple jumps),
        // so this is why I chose to simplify this down to turns.
        var turns = GetValidTurns(board, Side, null);
        Turn bestTurn;

        if (turns.Count == 1)
        {
            // If there's only one valid turn we must take this turn.
            bestTurn = turns[0];
        }
        else
        {
            var bestScore = 0;
            List<Turn> bestTurns = new();

            var sw = Stopwatch.StartNew();

            // The engine is limited on time rather than depth into the future.
            // When the engine has thought for too long the cancellationToken passed to this method is cancelled, signaling that we must stop.
            // The best move can only be determined after a complete search at a certain depth, so we must start with smaller depths and move on to greater depths if we have time.
            var depth = 2;
            while (!stoppingToken.IsCancellationRequested && depth <= 16)
            {
                depth += 2;
                try
                {
                    foreach (var turn in turns)
                        turn.Score = MiniMax(turn, depth, int.MinValue, int.MaxValue, false, stoppingToken);

                    bestScore = turns.Max(x => x.Score);
                    bestTurns = turns.Where(x => x.Score == bestScore).ToList();

                    // The next iteration will cull more nodes in alpha-beta pruning if we search better moves first.
                    turns = turns.OrderBy(x => x.Score);
                }
                catch (OperationCanceledException)
                {
                    depth -= 2;
                }
            }

            sw.Stop();
            _logger.LogInformation("Finished searching in {StopwatchElapsed}ms. Depth achieved: {Depth}, Best score: {BestScore}, Turns with this score: {BestTurnsCount}", sw.ElapsedMilliseconds, depth, bestScore, bestTurns.Count);

            // If multiple turns have the same score, take a random one
            bestTurn = bestTurns[_random.Next(0, bestTurns.Count)];
        }

        foreach (var move in bestTurn.Moves.Skip(1))
            _moveQueue.Enqueue(move);

        return bestTurn.Moves[0];
    }

    private List<Turn> GetValidTurns(Board board, int side, Turn parentTurn)
    {
        // Recursive algorithm to find the root nodes of the possible moves tree that could be made by the current player on one turn
        // Most branches will have length 1, but in the case of double-jumps or triple-jumps etc the branches could be longer
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

            // Copy the parent node and add this move to it, or if there is no parent node create a new node with this move
            var turn = parentTurn?.CloneAndAddMove(move) ?? new Turn(move);
            var childTurns = GetValidTurns(newBoard, side, turn);

            turns.AddRange(childTurns);
        }

        return turns;
    }

    // MiniMax algorithm with Alpha-Beta pruning recursively searches the tree of turns into the future to determine which path leads to the best outcome
    // It assumes that both players take the best moves so will avoid any paths that give the opponent an opportunity to make a great move, even if all their other moves are massively in favour of the computer.
    // I used the Wikipedia article https://en.wikipedia.org/wiki/Alpha-beta_pruning to help me implement this algorithm.
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

    // Statically evaluates a board's score for the current Side
    private int RateBoard(Board board, int depth)
    {
        // If someone won, return extreme score values to indicate very desirable or very undesirable
        // Offset this value by the depth to win as fast as possible or lose as slow as possible
        if (board.Winner == Side) return int.MaxValue - 16 + depth;
        if (board.Winner == 1 - Side) return int.MinValue + 16 - depth;

        var pieceScore = 0;
        var advancementScore = 0;

        // Scan through each tile on the board
        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                var tile = board.Tiles[y][x];
                if (tile == -1) continue;

                // Advancement signifies how close to being promoted this piece is
                int advancement;
                if (tile > 1) advancement = 7;           // If the piece is a king (>1) its advancement is the maximum (7) as it is already promoted
                else if (tile % 2 == 0) advancement = y; // Otherwise if the piece is black its advancement = y
                else advancement = 7 - y;                // Otherwise its advancement = 7 - y because white pieces advance in the opposite direction

                // Give a reward for keeping pieces on the home row to prevent enemy promotion
                if (advancement == 0) advancement = 7;

                if (board.Tiles[y][x] % 2 == Side)
                {
                    // If this piece is ours add the calculated scores to the totals
                    pieceScore++;
                    advancementScore += advancement;
                }
                else
                {
                    // Otherwise subtract the calculated scores from the totals
                    pieceScore--;
                    advancementScore -= advancement;
                }
            }
        }

        // Return the final score by weighting the piece score and advancement score in a 10:1 ratio
        return pieceScore * 10 + advancementScore;
    }

    // A "turn" is a move or set of moves (in the case of multiple jumps) that fully describe what a player does on their turn
    // The MiniMax algorithm can not cope with one player making multiple moves in sequence (in the case of multiple jumps),
    // so this is why I chose to simplify this down to turns.
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
