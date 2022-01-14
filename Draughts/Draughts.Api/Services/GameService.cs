using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Draughts.Api.Entities;
using Draughts.Api.Games;
using Microsoft.Extensions.DependencyInjection;

namespace Draughts.Api.Services;

public class GameService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Random _random;
    private ConcurrentDictionary<string, IGame> _games;

    public GameService(IServiceProvider serviceProvider, Random random)
    {
        _serviceProvider = serviceProvider;
        _random = random;
        _games = new();
    }

    public string CreateGame(GameOptions options)
    {
        // Create a new game depending on the type specified by the options
        // The games must be requested from dependency injection now rather than
        // in the GameService constructor because if we did that, only one game
        // instance would be created per game type.

        IGame game = options.GameType switch
        {
            GameType.LocalMultiplayer => _serviceProvider.GetRequiredService<LocalMultiplayerGame>(),
            GameType.OnlineMultiplayer => _serviceProvider.GetRequiredService<OnlineMultiplayerGame>(),
            GameType.Computer => _serviceProvider.GetRequiredService<ComputerGame>(),
            _ => throw new ArgumentOutOfRangeException(nameof(options.GameType))
        };
        game.Options = options;

        // This program is multi-threaded so two games can theoretically be created at once.
        // If this happened then both games could theoretically get the same game code, causing problems.
        // The lock keyword ensures that only one thread can enter the block at once, solving this issue.

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
        // Games become "redundant" when they have ended, but they have no way of self-destructing.
        // This method removes all redundant games and is called when a player leaves a game.

        foreach (var redundantGame in _games.Where(x => x.Value.IsRedundant))
            _games.TryRemove(redundantGame);
    }

    private string GetCode()
    {
        string code;

        // Generate a random 6-letter random code repeatedly until one is found which no game is currently using.

        // The do ... while (condition) syntax is rare, but it's basically
        // a while loop that always executes once before checking the condition.

        do
        {
            // Use a StringBuilder rather than a string for performance optimisation.
            // A string will allocate an entire new string each time it is appended to,
            // whereas a StringBuilder will not.

            var codeBuilder = new StringBuilder();

            for (var i = 0; i < 6; i++)
            {
                // Append a random character A-Z to the code builder
                var ascii = _random.Next(65, 91);
                var character = (char)ascii;
                codeBuilder.Append(character);
            }

            code = codeBuilder.ToString();
        } while (_games.Any(x => x.Key == code));

        return code;
    }
}
