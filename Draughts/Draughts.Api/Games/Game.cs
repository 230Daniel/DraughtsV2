using System.Threading.Tasks;
using Draughts.GameLogic;
using Microsoft.AspNetCore.SignalR;

namespace Draughts.Api.Games
{
    public class Game
    {
        public GameStatus GameStatus { get; private set; }
        public Board Board { get; private set; }
        
        private IClientProxy _playerConnection;

        public void AddPlayer(IClientProxy connection)
        {
            _playerConnection = connection;
            GameStatus = GameStatus.Waiting;
        }

        public async Task OnReadyAsync()
        {
            GameStatus = GameStatus.Playing;
            Board = new Board();
            await _playerConnection.SendAsync("GAME_STARTED", this);
        }
    }
    
    public enum GameStatus
    {
        Default,
        Waiting,
        Playing,
        Finished,
        Canceled
    }
}
