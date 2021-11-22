using System.Threading.Tasks;
using AutoMapper;
using Draughts.Api.Games;
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
        public string CreateGame(CreateGameModel createGameModel)
        {
            var code = _gameService.CreateGame(createGameModel.GameType);
            return code;
        }

        [HubMethodName("VALIDATE_GAME_CODE")]
        public bool ValidateGameCode(string code)
        {
            var game = _gameService.GetGame(code);
            return game is not null && game.IsJoinable;
        }
        
        [HubMethodName("JOIN_GAME")]
        public async Task<JoinResponse> JoinGameAsync(string code)
        {
            var game = _gameService.GetGame(code);
            var joinResponse = await game.OnJoinAsync(Context.ConnectionId);
            return joinResponse;
        }

        [HubMethodName("TAKE_MOVE")]
        public async Task TakeMoveAsync(string code, int[] origin, int[] destination)
        {
            var game = _gameService.GetGame(code);
            await game.OnTakeMoveAsync(Context.ConnectionId, (origin[0], origin[1]), (destination[0], destination[1]));
        }
    }
}
