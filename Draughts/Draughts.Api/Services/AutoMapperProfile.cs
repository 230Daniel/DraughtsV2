using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Draughts.Api.Entities;
using Draughts.Api.Games;
using Draughts.Api.Models;
using Draughts.GameLogic;

namespace Draughts.Api.Services
{
    /// <summary>
    ///     Defines rules that IMapper follows to convert between different types.
    ///     This is useful for converting complex classes into classes that can be sent over JSON to the frontend.
    /// </summary>
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
            CreateMap<IGame, GameModel>();
            CreateMap<CreateGameModel, GameOptions>()
                .ForMember(entity => entity.CreatorSide, member => member.MapFrom(model => model.Side));
        }
    }
}
