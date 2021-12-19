using System.Threading;
using Draughts.GameLogic;

namespace Draughts.Api.Services
{
    public interface IEngine
    {
        public int Side { get; set; }

        public ((int, int), (int, int)) GetMove(Board board, CancellationToken stoppingToken);
    }
}
