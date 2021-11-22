using System.Collections.Generic;
using System.Threading.Tasks;
using Draughts.Api.Entities;
using Draughts.GameLogic;

namespace Draughts.Api.Games
{
    public interface IGame
    {
        int Type { get; }
        string Code { get; set; }
        GameOptions Options { get; set; }
        Board Board { get; }
        IEnumerable<string> PlayerConnectionIds { get; }
        public bool IsRedundant { get; }

        Task<bool> OnJoinAsync(string connectionId);
        Task<int> OnReadyAsync(string connectionId);
        Task OnTakeMoveAsync(string connectionId, (int, int) origin, (int, int) destination);

        Task OnLeaveAsync(string connectionId);
    }
}
