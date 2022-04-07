using System.Threading;

namespace Draughts.GameLogic.Engines;

public interface IEngine
{
    public int Side { set; }

    public Move GetMove(Board board, CancellationToken stoppingToken);
}
