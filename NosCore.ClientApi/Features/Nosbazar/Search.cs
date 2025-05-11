using FluentResults;
using FluentValidation;
using Mapster;
using MediatR;
using Carter;
using Contracts.Responses.Nosbazar;
using Contracts.Requests.Nosbazar;
using NostaleSdk.Nosbazar;
using NosCore.ClientApi.Services.Nosbazar;
using NosCore.Packets.CustomPackets.Nosbazar;
using NosCore.Packets.ServerPackets.Auction;

namespace NosCore.ClientApi.Features.Nosbazar
{
    public static class Search
    {
        public class Command : IRequest<Result<SearchResponse>>
        {
            public int PageSearch { get; set; }
            public CategoryMainType MainType { get; set; }
            public CategoryClassType ClassType { get; set; }
            public CategoryLevelType LevelType { get; set; }
            public CategoryRarityType RarityType { get; set; }
            public CategoryUpgradeType UpgradeType { get; set; }
            public CategorySortType SortType { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.PageSearch).NotNull();
                RuleFor(c => c.MainType).NotNull();
                RuleFor(c => c.ClassType).NotNull();
                RuleFor(c => c.LevelType).NotNull();
                RuleFor(c => c.RarityType).NotNull();
                RuleFor(c => c.UpgradeType).NotNull();
                RuleFor(c => c.SortType).NotNull();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<SearchResponse>>
        {

            private readonly IValidator<Command> _validator;
            private readonly NosbazarService _nosbazarSearchService;

            public Handler(IValidator<Command> validator, NosbazarService nosbazarSearchService)
            {
                _validator = validator;
                _nosbazarSearchService = nosbazarSearchService;
            }

            public async Task<Result<SearchResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var ValidationResult = _validator.Validate(request);

                if (!ValidationResult.IsValid)
                {
                    return Result.Fail<SearchResponse>(new Error("ClientApi.Nosbazar.Search", new Error("Wrong parameters")));
                }

                var SearchRequest = request.Adapt<SearchRequest>();

                var SearchResult = await _nosbazarSearchService.SearchAsync(SearchRequest);

                if (SearchResult.IsSuccess)
                {
                    //var CustomPacket = RcBlistCustomPacket.RcBlistParser.Parse(SearchResult.Value.RcBlistObservedPacket.ReceivePacket);

                    return Result.Ok(SearchResult.Value);

                }

                return Result.Fail<SearchResponse>(SearchResult.Errors);
            }
        }
    }

    public class AccountsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("client/nosbazar/search", async (SearchRequest request, ISender sender) =>
            {
                var Command = request.Adapt<Search.Command>();

                var Result = await sender.Send(Command);

                if (Result.IsFailed)
                {
                    return Results.BadRequest(Result.Errors);
                }

                return Results.Ok(Result.Value);
            });
        }
    }
}
