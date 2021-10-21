using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Draughts.Api.Games;
using Draughts.Api.Models;
using Draughts.GameLogic;

namespace Draughts.Api.Services
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<List<((int, int), (int, int))>, int[][][]>()
                .ConvertUsing(moves => moves.Select(move =>
                    new[]
                    {
                        new[] {move.Item1.Item1, move.Item1.Item2},
                        new[] {move.Item2.Item1, move.Item2.Item2},
                    }).ToArray()
                );
            
            CreateMap<Board, BoardModel>();
            CreateMap<Game, GameModel>();
        }
    }
}
