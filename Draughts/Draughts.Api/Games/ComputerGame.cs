using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Draughts.Api.Entities;
using Draughts.Api.Hubs;
using Draughts.Api.Models;
using Draughts.Api.Services;
using Draughts.GameLogic;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace Draughts.Api.Games;

public class ComputerGame : IGame
{
    public int Type => 2;
    public string Code { get; set; }
    public GameOptions Options { get; set; }
    public Board Board { get; private set; }
    public IEnumerable<string> PlayerConnectionIds => new[] { _humanPlayer };
    public bool IsRedundant => _status == GameStatus.Redundant;

    private GameModel GameModel => _mapper.Map<GameModel>(this);
    private IClientProxy Clients => _hub.Clients.Group(Code);
        
    private readonly IServiceProvider _services;
    private readonly IHubContext<DraughtsHub> _hub;
    private readonly IMapper _mapper;
    private readonly Random _random;

    private string _humanPlayer;
    private int _humanPlayerSide;
    private IEngine _engine;
    
    private GameStatus _status;

    public ComputerGame(IServiceProvider services, IHubContext<DraughtsHub> hub, IMapper mapper, Random random)
    {
        _services = services;
        _hub = hub;
        _mapper = mapper;
        _random = random;
    }

    public async Task<bool> OnJoinAsync(string connectionId)
    {
        if (_status != GameStatus.WaitingForJoin) return false;
        _status = GameStatus.WaitingForReady;
        
        _humanPlayerSide = (int) Options.CreatorSide;
        if (_humanPlayerSide == -1) _humanPlayerSide = _random.Next(0, 2);
        
        // Set the player ID and tell the client to go to the game page
        _humanPlayer = connectionId;
        await _hub.Groups.AddToGroupAsync(connectionId, Code);
        await Clients.SendAsync("GAME_STARTED");
        
        return true;
    }

    public async Task<int> OnReadyAsync(string connectionId)
    {
        if (_status != GameStatus.WaitingForReady) return -1;
        _status = GameStatus.Playing;
            
        // The client is now on the game page, start the game of Draughts
        Board = new Board();
        await Clients.SendAsync("GAME_UPDATED", GameModel);
        
        // Set the computer engine to the selected opponent
        _engine = Options.Engine switch
        {
            Engine.MiniMax => _services.GetRequiredService<MiniMaxEngine>(),
            Engine.Random => _services.GetRequiredService<RandomEngine>(),
            _ => throw new ArgumentOutOfRangeException()
        };
        _engine.Side = 1 - _humanPlayerSide;
        
        // If the computer goes first, start calculating a move
        if (Board.NextPlayer == 1 - _humanPlayerSide)
            await TakeComputerMoveAsync();
        
        return _humanPlayerSide;
    }

    public async Task OnTakeMoveAsync(string connectionId, (int, int) origin, (int, int) destination)
    {
        if (_status != GameStatus.Playing)
            return;

        var nextPlayer = Board.NextPlayer == _humanPlayerSide ? _humanPlayer : "COMPUTER";
        if (nextPlayer != connectionId) return;
        
        // When the client submits a move, pass it to the board and send the game updated event
        Board.TakeMove(origin, destination);
        if (Board.Winner != -1) _status = GameStatus.Finished;
        await Clients.SendAsync("GAME_UPDATED", GameModel);

        // If it's the computer's turn, start calculating a move
        if (Board.NextPlayer == 1 - _humanPlayerSide)
            await TakeComputerMoveAsync();
    }

    public async Task OnLeaveAsync(string connectionId)
    {
        if (_status == GameStatus.Redundant || !PlayerConnectionIds.Contains(connectionId)) return;
        _status = GameStatus.Redundant;

        // When the player disconnects tell all other clients that the player has left
        await Clients.SendAsync("PLAYER_LEFT");
    }

    private async Task TakeComputerMoveAsync()
    {
        // This method should run in the background if not awaited
        await Task.Yield();
        
        // The computer can't take its move too quickly or the frontend won't be able to animate it properly
        var minimumDelay = Task.Delay(500);
        
        // Create a cancellation token which gets canceled after the maximum thinking time
        var stoppingTokenSource = new CancellationTokenSource();
        stoppingTokenSource.CancelAfter(Options.EngineThinkingTime);
        
        // Get the engine to calculate the best move in the time it has
        var move = _engine.GetMove(Board, stoppingTokenSource.Token);
        
        await minimumDelay;
        await OnTakeMoveAsync("COMPUTER", move.Item1, move.Item2);
    }
    
    private enum GameStatus
    {
        WaitingForJoin,
        WaitingForReady,
        Playing,
        Finished,
        Redundant
    }
}
