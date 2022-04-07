using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Draughts.Api.Entities;
using Draughts.Api.Games;
using Draughts.Api.Models;
using Draughts.GameLogic;

namespace Draughts.Api.Services;

/// <summary>
///     Defines rules that IMapper follows to convert between different types.
///     This is useful for converting complex classes into classes that can be sent over JSON to the frontend.
/// </summary>
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<List<Move>, int[][][]>()
            .ConvertUsing(moves => moves.Select(move =>
                    new[]
                    {
                        new[] { move.Origin.X, move.Origin.Y },
                        new[] { move.Destination.X, move.Destination.Y },
                    }).ToArray()
            );

        CreateMap<int, TimeSpan>()
            .ConvertUsing(integer => TimeSpan.FromMilliseconds(integer));

        CreateMap<Board, BoardModel>();
        CreateMap<IGame, GameModel>();
        CreateMap<CreateGameModel, GameOptions>()
            .ForMember(entity => entity.CreatorSide, member => member.MapFrom(model => model.Side));
    }
}
