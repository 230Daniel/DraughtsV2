using System.Threading.Tasks;
using Draughts.GameLogic;

namespace Draughts.Api.Games
{
    public interface IGame
    {
        int Type { get; }
        string Code { get; set; }
        Board Board { get; }

        Task<bool> OnJoinAsync(string connectionId);
        Task<int> OnReadyAsync(string connectionId);
        Task OnTakeMoveAsync(string connectionId, (int, int) origin, (int, int) destination);
    }
}
