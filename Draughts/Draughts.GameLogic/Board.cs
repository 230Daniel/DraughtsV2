using System;
using System.Collections.Generic;
using System.Linq;

namespace Draughts.GameLogic;

public class Board
{
    /// <summary>
    ///     Represents all of the playable tiles on the board.
    ///     -1 = Nothing, 0 = Black pawn, 1 = White pawn, 2 = Black king, 3 = White king
    /// </summary>
    public int[][] Tiles { get; private set; }
        
    /// <summary>
    ///     The player whose turn it is to move.
    ///     0 = Black, 1 = White
    /// </summary>
    public int NextPlayer { get; private set; }
        
    /// <summary>
    ///     A list of moves which the next player could make.
    /// </summary>
    public List<Move> ValidMoves { get; private set; }

    /// <summary>
    ///     A boolean which represents whether or not the next move must be a jump.
    ///     This is useful for highlighting mandatory jump moves on the frontend.
    /// </summary>
    public bool NextMoveMustBeJump { get; private set; }

    /// <summary>
    ///     A list of moves which describes the current turn's moves.
    ///     More than one move can be taken per turn if the player has jumped a piece and can jump again.
    ///     This is useful for highlighting the previous move on the frontend.
    /// </summary>
    public List<Move> TurnMoves { get; private set; }

    /// <summary>
    ///     The player who won the game.
    ///     -1 = Currently playing, 0 = Black, 1 = White
    /// </summary>
    public int Winner { get; private set; }
        
    private bool _shouldTurnMovesReset;
        
    public Board()
    {
        Tiles = new[]
        {
            new [] {  0,  0,  0,  0 },
            new [] {  0,  0,  0,  0 },
            new [] {  0,  0,  0,  0 },
            new [] { -1, -1, -1, -1 },
            new [] { -1, -1, -1, -1 },
            new [] {  1,  1,  1,  1 },
            new [] {  1,  1,  1,  1 },
            new [] {  1,  1,  1,  1 }
            
            /*new [] { -1, -1, -1, -1 },
            new [] { -1, -1, -1, -1 },
            new [] { -1,  0, -1, -1 },
            new [] { -1,  1,  1, -1 },
            new [] { -1, -1, -1, -1 },
            new [] { -1, -1,  1, -1 },
            new [] { -1, -1, -1, -1 },
            new [] { -1, -1, -1, -1 }*/
        };
        Winner = -1;
        ValidMoves = GetValidMoves();
        TurnMoves = new();
    }

    private Board(
        int[][] tiles, 
        int winner, 
        int nextPlayer, 
        List<Move> validMoves, 
        bool nextMoveMustBeJump, 
        bool shouldTurnMovesReset)
    {
        Tiles = tiles;
        Winner = winner;
        NextPlayer = nextPlayer;
        ValidMoves = validMoves;
        NextMoveMustBeJump = nextMoveMustBeJump;
        _shouldTurnMovesReset = shouldTurnMovesReset;
    }

    public bool TakeMove(Coords origin, Coords destination) 
        => TakeMove(new Move(origin, destination));
    
    public bool TakeMove(Move move)
    {
        if (!ValidMoves.Contains(move)) return false;

        if (TurnMoves is not null)
        {
            if (_shouldTurnMovesReset)
            {
                TurnMoves.Clear();
                _shouldTurnMovesReset = false;
            }
            TurnMoves.Add(move);
        }

        var tileValue = Tiles[move.Origin.Y][move.Origin.X];
            
        // If a pawn has moved onto a home row, promote it
        var promoted = false;
        if (tileValue < 2 && move.Destination.Y is 0 or 7)
        {
            promoted = true;
            tileValue += 2;
        }

        Tiles[move.Destination.Y][move.Destination.X] = tileValue;
        Tiles[move.Origin.Y][move.Origin.X] = -1;

        // If the move was a take
        var yDifference = move.Destination.Y - move.Origin.Y; 
        if (Math.Abs(yDifference) == 2)
        {
            // Find the tile between the origin and destination and remove the piece
            var jumpedY = move.Origin.Y + yDifference / 2;
                
            var jumpedX = move.Destination.X < move.Origin.X
                ? move.Origin.X + jumpedY % 2 - 1 // If the jump was to the left
                : move.Origin.X + jumpedY % 2;    // If the jump was to the right
                
            Tiles[jumpedY][jumpedX] = -1;

            // If the piece that just moved has another taking move, let the player take another turn
            // This is not allowed if the piece was promoted this turn
            if (!promoted)
            {
                ValidMoves = GetValidMoves(true).Where(x => x.Origin == move.Destination).ToList();
                if (ValidMoves.Count > 0) return true;
            }
        }
            
        // Change the next player around, calculate the new valid moves, and set the flag 
        NextPlayer = 1 - NextPlayer;
        ValidMoves = GetValidMoves();
        _shouldTurnMovesReset = true;
        CheckForWinner();
        return true;
    }

    private List<Move> GetValidMoves(bool ignoreSingleMoves = false)
    {
        var singleMoves = new List<Move>();
        var jumpingMoves = new List<Move>();

        for (var y = 0; y < 8; y++)
        {
            for (var x = 0; x < 4; x++)
            {
                var tile = Tiles[y][x];

                // If the tile is empty or doesn't belong to the player to move, skip it
                if (tile == -1 || tile % 2 != NextPlayer) continue;
                    
                // Offset the value of X to account for the non-playable tile at the start of every other row
                var offsetX = x + 1 - y % 2;
                    
                // If the tile is black or a white king
                if (tile % 2 == 0 || tile == 3)
                {
                    // If there's a tile up and left
                    if (y < 7 && offsetX > 0)
                    {
                        var newTile = Tiles[y + 1][offsetX - 1];
                        if (newTile == -1) singleMoves.Add(new Move(new Coords(x, y), new Coords(offsetX - 1, y + 1)));
                            
                        // If the piece on the new tile is an opponent's piece and there's an empty tile to jump over it to
                        else if (newTile % 2 != NextPlayer && x > 0 && y < 6 && Tiles[y + 2][x - 1] == -1)
                        {
                            jumpingMoves.Add(new Move(new Coords(x, y), new Coords(x - 1, y + 2)));
                        }
                    }
                    
                    // If there's a tile up and right
                    if (y < 7 && offsetX < 4)
                    {
                        var newTile = Tiles[y + 1][offsetX];
                        if (newTile == -1) singleMoves.Add(new Move(new Coords(x, y), new Coords(offsetX, y + 1)));
                            
                        // If the piece on the new tile is an opponent's piece and there's an empty tile to jump over it to
                        else if (newTile % 2 != NextPlayer && x < 3 && y < 6 && Tiles[y + 2][x + 1] == -1)
                        {
                            jumpingMoves.Add(new Move(new Coords(x, y), new Coords(x + 1, y + 2)));
                        }
                    }
                }
                    
                // If the tile is white or a black king
                if (tile % 2 == 1 || tile == 2)
                {
                    // If there's a tile down and left
                    if (y > 0 && offsetX > 0)
                    {
                        var newTile = Tiles[y - 1][offsetX - 1];
                        if (newTile == -1) singleMoves.Add(new Move(new Coords(x, y), new Coords(offsetX - 1, y - 1)));
                            
                        // If the piece on the new tile is an opponent's piece and there's an empty tile to jump over it to
                        else if (newTile % 2 != NextPlayer && x > 0 && y > 1 && Tiles[y - 2][x - 1] == -1)
                        {
                            jumpingMoves.Add(new Move(new Coords(x, y), new Coords(x - 1, y - 2)));
                        }
                    }
                    
                    // If there's a tile down and right
                    if (y > 0 && offsetX < 4)
                    {
                        var newTile = Tiles[y - 1][offsetX];
                        if (newTile == -1) singleMoves.Add(new Move(new Coords(x, y), new Coords(offsetX, y - 1)));
                            
                        // If the piece on the new tile is an opponent's piece and there's an empty tile to jump over it to
                        else if (newTile % 2 != NextPlayer && x < 3 && y > 1 && Tiles[y - 2][x + 1] == -1)
                        {
                            jumpingMoves.Add(new Move(new Coords(x, y), new Coords(x + 1, y - 2)));
                        }
                    }
                }
            }
        }

        NextMoveMustBeJump = jumpingMoves.Count > 0;
        // If there are any jumping moves, all single moves would be invalid
        return ignoreSingleMoves || jumpingMoves.Count > 0 ? jumpingMoves : singleMoves;
    }
        
    private void CheckForWinner()
    {
        // If the next player has no valid moves, the other player has won
        if (ValidMoves.Count == 0)
            Winner = 1 - NextPlayer;
    }

    public Board Clone()
    {
        // Create a new board with the same properties as this one
        // which can be altered independently.
        return new Board(
            Tiles.Select(x => (int[]) x.Clone()).ToArray(),
            Winner,
            NextPlayer,
            new List<Move>(ValidMoves),
            NextMoveMustBeJump,
            _shouldTurnMovesReset);
    }
}
