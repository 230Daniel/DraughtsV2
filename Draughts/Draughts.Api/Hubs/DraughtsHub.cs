using System.Threading.Tasks;
using Draughts.Api.Games;
using Draughts.Api.Services;
using Microsoft.AspNetCore.SignalR;

namespace Draughts.Api.Hubs
{
    public class DraughtsHub : Hub
    {
        private readonly GameService _gameService;
        
        public DraughtsHub(GameService gameService)
        {
            _gameService = gameService;
        }
        
        [HubMethodName("CREATE_GAME")]
        public Game CreateGame()
        {
            // Create a new game and add the player connection to it
            var game = _gameService.CreateGame(Context.ConnectionId);
            game.AddPlayer(Clients.Caller);
            return game;
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
