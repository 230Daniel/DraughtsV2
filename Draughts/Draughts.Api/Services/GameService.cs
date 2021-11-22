using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Draughts.Api.Entities;
using Draughts.Api.Games;
using Microsoft.Extensions.DependencyInjection;

namespace Draughts.Api.Services
{
    public class GameService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random;
        private readonly char[] _codeCharacters;
        private ConcurrentDictionary<string, IGame> _games;

        public GameService(IServiceProvider serviceProvider, Random random)
        {
            _serviceProvider = serviceProvider;
            _random = random;
            _codeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            _games = new();
        }
        
        public string CreateGame(GameOptions options)
        {
            IGame game = options.GameType switch
            {
                GameType.LocalMultiplayer => _serviceProvider.GetRequiredService<LocalMultiplayerGame>(),
                GameType.OnlineMultiplayer => _serviceProvider.GetRequiredService<OnlineMultiplayerGame>(),
                _ => throw new ArgumentOutOfRangeException(nameof(options.GameType))
            };
            game.Options = options;
            
            lock (_games)
            {
                var code = GetCode();
                _games.TryAdd(code, game);
                game.Code = code;
                return code;
            }
        }

        public IGame GetGame(string code)
        {
            return _games.GetValueOrDefault(code);
        }

        public IEnumerable<IGame> GetGamesForConnection(string connectionId)
        {
            return _games.Values.Where(x => x.PlayerConnectionIds.Contains(connectionId));
        }

        public void RemoveRedundantGames()
        {
            foreach (var redundantGame in _games.Where(x => x.Value.IsRedundant))
            {
                _games.TryRemove(redundantGame);
            }
        }

        private string GetCode()
        {
            string code;

            do
            {
                var codeBuilder = new StringBuilder();
                for (var i = 0; i < 6; i++)
                    codeBuilder.Append(_codeCharacters[_random.Next(0, _codeCharacters.Length)]);
                code = codeBuilder.ToString();
            } 
            while (_games.Any(x => x.Key == code));

            return code;
        }
    }
}
