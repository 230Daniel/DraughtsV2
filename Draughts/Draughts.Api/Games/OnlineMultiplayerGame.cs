using System.Threading.Tasks;
using AutoMapper;
using Draughts.Api.Hubs;
using Draughts.Api.Models;
using Draughts.GameLogic;
using Microsoft.AspNetCore.SignalR;

namespace Draughts.Api.Games
{
    public class OnlineMultiplayerGame : IGame
    {
        public int Type => 1;
        public string Code { get; set; }
        public Board Board { get; private set; }

        private GameModel GameModel => _mapper.Map<GameModel>(this);
        private IClientProxy Clients => _hub.Clients.Group(Code);
        
        private readonly IMapper _mapper;
        private readonly IHubContext<DraughtsHub> _hub;
        
        private Player _player1;
        private Player _player2;
        private GameStatus _status;

        public OnlineMultiplayerGame(IHubContext<DraughtsHub> hub, IMapper mapper)
        {
            _hub = hub;
            _mapper = mapper;
        }

        public async Task<bool> OnJoinAsync(string connectionId)
        {
            if (_status != GameStatus.WaitingForJoin) return false;
            
            if (_player1 is null)
            {
                _player1 = new Player(connectionId);
                await _hub.Groups.AddToGroupAsync(connectionId, Code);
                return true;
            }
            
            _player2 = new Player(connectionId);
            _status = GameStatus.WaitingForReady;
            await _hub.Groups.AddToGroupAsync(connectionId, Code);
            await Clients.SendAsync("GAME_STARTED");

            return true;
        }

        public async Task<int> OnReadyAsync(string connectionId)
        {
            if (_status != GameStatus.WaitingForReady) return -1;
            var playerNumber = connectionId == _player1.ConnectionId ? 0 : connectionId == _player2.ConnectionId ? 1 : -1;
            if (playerNumber == 0) _player1.IsReady = true;
            if (playerNumber == 1) _player2.IsReady = true;

            if (_player1.IsReady && _player2.IsReady)
            {
                Board = new Board();
                _status = GameStatus.Playing;
                await Clients.SendAsync("GAME_UPDATED", GameModel);
            }

            return playerNumber;
        }
        
        public async Task OnTakeMoveAsync(string connectionId, (int, int) origin, (int, int) destination)
        {
            if (_status != GameStatus.Playing) return;
            
            var nextPlayer = Board.NextPlayer == 0 ? _player1 : _player2;
            if (nextPlayer.ConnectionId != connectionId) return;
            
            // When the client submits a move, take it on the board and then send the game updated event
            Board.TakeMove(origin, destination);
            await Clients.SendAsync("GAME_UPDATED", GameModel);
        }

        private enum GameStatus
        {
            WaitingForJoin,
            WaitingForReady,
            Playing
        }

        private class Player
        {
            public string ConnectionId { get; }
            public bool IsReady { get; set; }

            public Player(string connectionId)
            {
                ConnectionId = connectionId;
            }
        }
    }
}
