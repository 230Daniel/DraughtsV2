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
        /// <summary>
        ///     The status of this local multiplayer game
        ///     0 = Waiting for player
        ///     1 = Playing
        /// </summary>
        public int Status { get; private set; }
        public bool IsJoinable => Status == 0;
        public Board Board { get; private set; }

        private GameModel GameModel => _mapper.Map<GameModel>(this);
        private IClientProxy Client => _hub.Clients.Group(Code);
        
        private readonly IHubContext<DraughtsHub> _hub;
        private readonly IMapper _mapper;

        private string _player;

        public LocalMultiplayerGame(IHubContext<DraughtsHub> hub, IMapper mapper)
        {
            _hub = hub;
            _mapper = mapper;
        }

        public async Task<JoinResponse> OnJoinAsync(string connectionId)
        {
            if (Status != 0)
                return new();

            _player = connectionId;
            await _hub.Groups.AddToGroupAsync(connectionId, Code);
            
            Status = 1;
            Board = new Board();
            
            await Client.SendAsync("GAME_UPDATED", GameModel);

            return new() {IsSuccess = true};
        }

        public async Task OnTakeMoveAsync(string connectionId, (int, int) origin, (int, int) destination)
        {
            if (Status != 1 || connectionId != _player) return;

            // When the client submits a move, take it on the board and then send the game updated event
            Board.TakeMove(origin, destination);
            await Client.SendAsync("GAME_UPDATED", GameModel);
        }
    }
}
