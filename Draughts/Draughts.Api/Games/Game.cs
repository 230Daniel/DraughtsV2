using System;
using System.Threading.Tasks;
using AutoMapper;
using Draughts.Api.Models;
using Draughts.GameLogic;
using Microsoft.AspNetCore.SignalR;

namespace Draughts.Api.Games
{
    public class Game
    {
        public GameStatus GameStatus { get; private set; }
        public Board Board { get; private set; }

        private GameModel GameModel => _mapper.Map<GameModel>(this);
        
        private readonly IMapper _mapper;
        private IClientProxy _playerConnection;
       

        public Game(IMapper mapper)
        {
            _mapper = mapper;
        }
        
        public void AddPlayer(IClientProxy connection)
        {
            // Set the player connection so we can send them events
            _playerConnection = connection;
            GameStatus = GameStatus.Waiting;
        }

        public async Task OnReadyAsync()
        {
            if (GameStatus != GameStatus.Waiting) 
                throw new InvalidOperationException();
            
            // When the client is ready to start, begin the game
            GameStatus = GameStatus.Playing;
            Board = new Board();
            await _playerConnection.SendAsync("GAME_UPDATED", GameModel);
        }
        
        public async Task OnTakeMoveAsync((int, int) origin, (int, int) destination)
        {
            if (GameStatus != GameStatus.Playing) 
                throw new InvalidOperationException();
            
            // When the client submits a move, take it on the board and then send the game updated event
            Board.TakeMove(origin, destination);
            await _playerConnection.SendAsync("GAME_UPDATED", GameModel);
        }
    }
    
    public enum GameStatus
    {
        Default,
        Waiting,
        Playing
    }
}
