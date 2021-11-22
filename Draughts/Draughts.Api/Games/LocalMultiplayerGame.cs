using System.Threading.Tasks;
using AutoMapper;
using Draughts.Api.Hubs;
using Draughts.Api.Models;
using Draughts.GameLogic;
using Microsoft.AspNetCore.SignalR;

namespace Draughts.Api.Games
{
    public class LocalMultiplayerGame : IGame
    {
        public int Type => 0;
        public string Code { get; set; }
        public Board Board { get; private set; }

        private GameModel GameModel => _mapper.Map<GameModel>(this);
        private IClientProxy Client => _hub.Clients.Group(Code);
        
        private readonly IHubContext<DraughtsHub> _hub;
        private readonly IMapper _mapper;

        private string _player;
        private GameStatus _status;

        public LocalMultiplayerGame(IHubContext<DraughtsHub> hub, IMapper mapper)
        {
            _hub = hub;
            _mapper = mapper;
        }

        public async Task<bool> OnJoinAsync(string connectionId)
        {
            if (_status != GameStatus.WaitingForJoin) return false;
            _status = GameStatus.WaitingForReady;
            
            _player = connectionId;
            await _hub.Groups.AddToGroupAsync(connectionId, Code);
            await Client.SendAsync("GAME_STARTED");

            return true;
        }

        public async Task<int> OnReadyAsync(string connectionId)
        {
            if (_status != GameStatus.WaitingForReady) return -1;
            _status = GameStatus.Playing;
            
            Board = new Board();
            await Client.SendAsync("GAME_UPDATED", GameModel);
            return 0;
        }

        public async Task OnTakeMoveAsync(string connectionId, (int, int) origin, (int, int) destination)
        {
            if (_status != GameStatus.Playing || connectionId != _player) return;

            // When the client submits a move, take it on the board and then send the game updated event
            Board.TakeMove(origin, destination);
            await Client.SendAsync("GAME_UPDATED", GameModel);
        }

        private enum GameStatus
        {
            WaitingForJoin,
            WaitingForReady,
            Playing
        }
    }
}
