using System.Threading.Tasks;
using Draughts.GameLogic;

namespace Draughts.Api.Games
{
    public interface IGame
    {
        int Type { get; }
        string Code { get; set; }
        int Status { get; }
        Board Board { get; }
        
        Task<JoinResponse> OnJoinAsync(string connectionId);
        Task OnTakeMoveAsync(string connectionId, (int, int) origin, (int, int) destination);
    }

    public class JoinResponse
    {
        public bool IsSuccess { get; init; }
        public int PlayerNumber { get; init; }
    }
}
