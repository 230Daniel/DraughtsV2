using System.Threading.Tasks;
using Draughts.Api.Models;
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
        public string CreateGame(CreateGameModel createGameModel)
        {
            var code = _gameService.CreateGame(createGameModel.GameType);
            return code;
        }

        [HubMethodName("JOIN_GAME")]
        public async Task<bool> JoinGameAsync(string code)
        {
            var game = _gameService.GetGame(code);
            if (game is null) return false;
            return await game.OnJoinAsync(Context.ConnectionId);
        }
        
        [HubMethodName("READY")]
        public async Task<int> ReadyAsync(string code)
        {
            var game = _gameService.GetGame(code);
            if (game is null) return -1;
            return await game.OnReadyAsync(Context.ConnectionId);
        }
        
        [HubMethodName("TAKE_MOVE")]
        public async Task TakeMoveAsync(string code, int[] origin, int[] destination)
        {
            var game = _gameService.GetGame(code);
            await game.OnTakeMoveAsync(Context.ConnectionId, (origin[0], origin[1]), (destination[0], destination[1]));
        }
    }
}
