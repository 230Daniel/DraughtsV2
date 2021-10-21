using System.Threading.Tasks;
using AutoMapper;
using Draughts.Api.Models;
using Draughts.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace Draughts.Api.Hubs
{
    public class DraughtsHub : Hub
    {
        private readonly IMapper _mapper;
        private readonly GameService _gameService;
        
        public DraughtsHub(IMapper mapper, GameService gameService)
        {
            _mapper = mapper;
            _gameService = gameService;
        }
        
        [HubMethodName("CREATE_GAME")]
        public GameModel CreateGame()
        {
            // Create a new game and add the player connection to it
            var game = _gameService.CreateGame(Context.ConnectionId);
            game.AddPlayer(Clients.Caller);
            return _mapper.Map<GameModel>(game);
        }

        [HubMethodName("READY")]
        public async Task ReadyAsync()
        {
            // Get this connection's game and call ready on it
            var game = _gameService.GetGame(Context.ConnectionId);
            await game.OnReadyAsync();
        }
    }
}
