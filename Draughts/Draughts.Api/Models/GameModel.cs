using Draughts.Api.Games;

namespace Draughts.Api.Models
{
    public class GameModel
    {
        public GameStatus GameStatus { get; init; }
        public BoardModel Board { get; init; }
    }
}
