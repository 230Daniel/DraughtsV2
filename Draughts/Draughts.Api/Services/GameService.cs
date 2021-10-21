using System;
using System.Collections.Generic;
using Draughts.Api.Games;
using Microsoft.Extensions.DependencyInjection;

namespace Draughts.Api.Services
{
    public class GameService
    {
        private readonly IServiceProvider _serviceProvider;
        private Dictionary<string, Game> _games;
        
        public GameService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _games = new();
        }
        
        public Game CreateGame(string connectionId)
        {
            // Remove any game that this connection created beforehand
            _games.Remove(connectionId);
            
            // Create a new game and add it to the dictionary
            var game = _serviceProvider.GetRequiredService<Game>();
            _games.Add(connectionId, game);
            return game;
        }

        public Game GetGame(string connectionId)
        {
            // Return the game belonging to this connection id, or null if there isn't one
            return _games.GetValueOrDefault(connectionId);
        }
    }
}
