namespace Draughts.Api.Models
{
    public class GameModel
    {
        public int Type { get; init; }
        public int Status { get; init; }
        public BoardModel Board { get; init; }
    }
}
