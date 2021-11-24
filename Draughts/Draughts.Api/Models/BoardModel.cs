namespace Draughts.Api.Models
{
    /// <summary>
    ///     Draughts.GameLogic.Board can not be serialized properly into JSON,
    ///     so the board is converted to this model before being sent.
    /// </summary>
    public class BoardModel
    {
        public int[][] Tiles { get; init; }
        public int NextPlayer { get; init; }
        public int[][][] ValidMoves { get; init; }
        public bool NextMoveMustBeJump { get; init; }
        public int[][][] TurnMoves { get; init; }
        public int Winner { get; init; }
    }
}
