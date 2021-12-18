using Draughts.GameLogic;

namespace Draughts.Api.Engines;

public interface IEngine
{
    public ((int, int), (int, int)) GetMove(Board board, int side);
}
