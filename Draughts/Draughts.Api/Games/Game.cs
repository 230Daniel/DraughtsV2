using System.Threading.Tasks;
using AutoMapper;
using Draughts.Api.Models;
using Draughts.GameLogic;
using Microsoft.AspNetCore.SignalR;

namespace Draughts.Api.Games
{
    public class Game
    {
        public GameStatus GameStatus { get; private set; }
        public Board Board { get; private set; }

        private readonly IMapper _mapper;
        private GameModel GameModel => _mapper.Map<GameModel>(this);
        private IClientProxy _playerConnection;
       

        public Game(IMapper mapper)
        {
            _mapper = mapper;
        }
        
        public void AddPlayer(IClientProxy connection)
        {
            _playerConnection = connection;
            GameStatus = GameStatus.Waiting;
        }

        public async Task OnReadyAsync()
        {
            GameStatus = GameStatus.Playing;
            Board = new Board();
            await _playerConnection.SendAsync("GAME_STARTED", GameModel);
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
