using System.Collections.Generic;
using System.Threading.Tasks;
using Draughts.Api.Entities;
using Draughts.GameLogic;

namespace Draughts.Api.Games;

public interface IGame
{
    /// <summary>
    ///     The type of game.
    ///     0 = Local multiplayer
    ///     1 = Online multiplayer
    /// </summary>
    int Type { get; }
        
    /// <summary>
    ///     The game code for joining.
    /// </summary>
    string Code { get; set; }
        
    /// <summary>
    ///     The game options set when the game was created.
    /// </summary>
    GameOptions Options { get; set; }
        
    /// <summary>
    ///     The current Draughts board.
    /// </summary>
    Board Board { get; }
        
    /// <summary>
    ///     A list of connected players.
    /// </summary>
    IEnumerable<string> PlayerConnectionIds { get; }
        
    /// <summary>
    ///     True if the game has finished and can be disposed.
    /// </summary>
    public bool IsRedundant { get; }

    Task<bool> OnJoinAsync(string connectionId);
    Task<int> OnReadyAsync(string connectionId);
    Task OnTakeMoveAsync(string connectionId, (int, int) origin, (int, int) destination);

    Task OnLeaveAsync(string connectionId);
}
