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
        /// <summary>
        ///     The status of this online multiplayer game.
        ///     0 = Waiting for player 1
        ///     1 = Waiting for player 2
        ///     2 = Playing
        /// </summary>
        public int Status { get; private set; }
        public Board Board { get; private set; }

        private GameModel GameModel => _mapper.Map<GameModel>(this);
        private IClientProxy Clients => _hub.Clients.Group(Code);
        
        private readonly IMapper _mapper;
        private readonly IHubContext<DraughtsHub> _hub;
        
        private string _player1;
        private string _player2;

        public OnlineMultiplayerGame(IHubContext<DraughtsHub> hub, IMapper mapper)
        {
            _hub = hub;
            _mapper = mapper;
        }

        public async Task<JoinResponse> OnJoinAsync(string connectionId)
        {
            if (Status == 0)
            {
                _player1 = connectionId;
                await _hub.Groups.AddToGroupAsync(connectionId, Code);
                
                Status = 1;
                await Clients.SendAsync("GAME_UPDATED", GameModel);
                
                return new() {IsSuccess = true, PlayerNumber = 0};
            }
            
            if (Status == 1)
            {
                _player2 = connectionId;
                await _hub.Groups.AddToGroupAsync(connectionId, Code);
                
                Status = 2;
                Board = new Board();
                await Clients.SendAsync("GAME_UPDATED", GameModel);

                return new() {IsSuccess = true, PlayerNumber = 1};
            }
            
            return new();
        }

        public async Task OnTakeMoveAsync(string connectionId, (int, int) origin, (int, int) destination)
        {
            if (Status != 2) return;
            
            var nextPlayer = Board.NextPlayer == 0 ? _player1 : _player2;
            if (nextPlayer != connectionId) return;
            
            // When the client submits a move, take it on the board and then send the game updated event
            Board.TakeMove(origin, destination);
            await Clients.SendAsync("GAME_UPDATED", GameModel);
        }
    }
}
