using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Draughts.Api.Games;
using Microsoft.Extensions.DependencyInjection;

namespace Draughts.Api.Services
{
    public class GameService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random;
        private readonly char[] _codeCharacters;
        private Dictionary<string, IGame> _games;

        public GameService(IServiceProvider serviceProvider, Random random)
        {
            _serviceProvider = serviceProvider;
            _random = random;
            _codeCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            _games = new();
        }
        
        public string CreateGame(int gameType)
        {
            IGame game = gameType switch
            {
                0 => _serviceProvider.GetRequiredService<LocalMultiplayerGame>(),
                1 => _serviceProvider.GetRequiredService<OnlineMultiplayerGame>(),
                _ => throw new ArgumentOutOfRangeException(nameof(gameType))
            };

            lock (_games)
            {
                var code = GetCode();
                _games.Add(code, game);
                game.Code = code;
                return code;
            }
        }

        public IGame GetGame(string code)
        {
            return _games.GetValueOrDefault(code);
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
