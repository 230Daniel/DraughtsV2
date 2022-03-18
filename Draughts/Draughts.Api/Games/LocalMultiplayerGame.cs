using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Draughts.Api.Entities;
using Draughts.Api.Hubs;
using Draughts.Api.Models;
using Draughts.GameLogic;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Draughts.Api.Games;

public class LocalMultiplayerGame : IGame
{
    public int Type => 0;
    public string Code { get; set; }
    public GameOptions Options { get; set; }
    public Board Board { get; private set; }
    public IEnumerable<string> PlayerConnectionIds => new[] { _player };
    public bool IsRedundant => _status == GameStatus.Redundant;

    private GameModel GameModel => _mapper.Map<GameModel>(this);
    private IClientProxy Clients => _hub.Clients.Group(Code);

    private readonly ILogger<LocalMultiplayerGame> _logger;
    private readonly IHubContext<DraughtsHub> _hub;
    private readonly IMapper _mapper;

    private string _player;
    private GameStatus _status;

    public LocalMultiplayerGame(ILogger<LocalMultiplayerGame> logger, IHubContext<DraughtsHub> hub, IMapper mapper)
    {
        _logger = logger;
        _hub = hub;
        _mapper = mapper;
    }

    public async Task<bool> OnJoinAsync(string connectionId)
    {
        if (_status != GameStatus.WaitingForJoin) return false;
        _status = GameStatus.WaitingForReady;

        // Set the player ID and tell the client to go to the game page
        _player = connectionId;
        await _hub.Groups.AddToGroupAsync(connectionId, Code);
        await Clients.SendAsync("GAME_STARTED");

        _logger.LogInformation("Local Multiplayer game {Code} started", Code);

        return true;
    }

    public async Task<int> OnReadyAsync(string connectionId)
    {
        if (_status != GameStatus.WaitingForReady) return -1;
        _status = GameStatus.Playing;

        // The client is now on the game page, start the game of Draughts
        Board = new Board();
        await Clients.SendAsync("GAME_UPDATED", GameModel);

        // Tell the client it's player 1, this will be ignored anyway for a local game.
        return 0;
    }

    public async Task OnTakeMoveAsync(string connectionId, Coords origin, Coords destination)
    {
        if (_status != GameStatus.Playing || connectionId != _player) return;

        // When the client submits a move, pass it to the board and send the game updated event
        Board.TakeMove(origin, destination);
        if (Board.Winner != -1)
        {
            _status = GameStatus.Finished;
            _logger.LogInformation("Local Multiplayer game {Code} won by player {Winner}", Code, Board.Winner);
        }
        await Clients.SendAsync("GAME_UPDATED", GameModel);
    }

    public async Task OnLeaveAsync(string connectionId)
    {
        if (_status == GameStatus.Redundant || !PlayerConnectionIds.Contains(connectionId)) return;
        _status = GameStatus.Redundant;

        _logger.LogInformation("Local Multiplayer game {Code} cancelled because the player left", Code);

        // When the player disconnects tell all other clients that the player has left
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
}
