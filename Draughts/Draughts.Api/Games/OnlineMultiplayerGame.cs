using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Draughts.Api.Entities;
using Draughts.Api.Hubs;
using Draughts.Api.Models;
using Draughts.GameLogic;
using Microsoft.AspNetCore.SignalR;

namespace Draughts.Api.Games;

public class OnlineMultiplayerGame : IGame
{
    public int Type => 1;
    public string Code { get; set; }
    public GameOptions Options { get; set; }
    public Board Board { get; private set; }
    public IEnumerable<string> PlayerConnectionIds => new[] { _player1?.ConnectionId, _player2?.ConnectionId };
    public bool IsRedundant => _status == GameStatus.Redundant;
        
    private GameModel GameModel => _mapper.Map<GameModel>(this);
    private IClientProxy Clients => _hub.Clients.Group(Code);
        
    private readonly IMapper _mapper;
    private readonly IHubContext<DraughtsHub> _hub;
    private readonly Random _random;
        
    private Player _player1;
    private Player _player2;
    private GameStatus _status;

    public OnlineMultiplayerGame(IHubContext<DraughtsHub> hub, IMapper mapper, Random random)
    {
        _hub = hub;
        _mapper = mapper;
        _random = random;
    }

    public async Task<bool> OnJoinAsync(string connectionId)
    {
        if (_status != GameStatus.WaitingForJoin) return false;

        if (_player1 is null && _player2 is null)
        {
            // If this is the first player to join set their side depending on the game options
            var creatorSide = (int) Options.CreatorSide;
            if (creatorSide == -1) creatorSide = _random.Next(0, 2);

            if (creatorSide == 0) _player1 = new Player(connectionId);
            else _player2 = new Player(connectionId);
                
            await _hub.Groups.AddToGroupAsync(connectionId, Code);
            return true;
        }
            
        // If this is the second player to join set their side to the opposite of the first player
        if (_player1 is null) _player1 = new Player(connectionId);
        else _player2 = new Player(connectionId);
        await _hub.Groups.AddToGroupAsync(connectionId, Code);
            
        _status = GameStatus.WaitingForReady;
        await Clients.SendAsync("GAME_STARTED");

        return true;
    }

    public async Task<int> OnReadyAsync(string connectionId)
    {
        if (_status != GameStatus.WaitingForReady) return -1;
            
        // Depending on which player sent the ready signal, set that player to ready
        var playerNumber = connectionId == _player1.ConnectionId ? 0 : connectionId == _player2.ConnectionId ? 1 : -1;
        if (playerNumber == 0) _player1.IsReady = true;
        if (playerNumber == 1) _player2.IsReady = true;

        if (_player1.IsReady && _player2.IsReady)
        {
            // If both players are ready start the game of Draughts
            Board = new Board();
            _status = GameStatus.Playing;
            await Clients.SendAsync("GAME_UPDATED", GameModel);
        }

        return playerNumber;
    }
        
    public async Task OnTakeMoveAsync(string connectionId, (int, int) origin, (int, int) destination)
    {
        if (_status != GameStatus.Playing) return;
            
        var nextPlayer = Board.NextPlayer == 0 ? _player1 : _player2;
        if (nextPlayer.ConnectionId != connectionId) return;
            
        // When the client submits a move, take it on the board and then send the game updated event
        Board.TakeMove(origin, destination);
        if (Board.Winner != -1) _status = GameStatus.Finished;
        await Clients.SendAsync("GAME_UPDATED", GameModel);
    }

    public async Task OnLeaveAsync(string connectionId)
    {
        if (_status == GameStatus.Redundant || !PlayerConnectionIds.Contains(connectionId)) return;
        _status = GameStatus.Redundant;
            
        // When a player disconnects tell all other clients that the player has left
        await Clients.SendAsync("PLAYER_LEFT");
    }

    private enum GameStatus
    {
        WaitingForJoin,
        WaitingForReady,
        Playing,
        Finished,
        Redundant
    }

    private class Player
    {
        public string ConnectionId { get; }
        public bool IsReady { get; set; }

        public Player(string connectionId)
        {
            ConnectionId = connectionId;
        }
    }
}
