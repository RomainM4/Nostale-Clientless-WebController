using Carter;
using Contracts.Requests.Authentication;
using Contracts.Responses.Authentication;
using FluentResults;
using FluentValidation;
using Mapster;
using MediatR;
using NosCore.AuthApi.Contracts;
using NosCore.AuthApi.HttpClients.Interfaces;

namespace NosCore.AuthApi.Features.Authentication
{
    public static class Challenge
    {
        public class Command : IRequest<Result<ChallengeResponse>>
        {
            public string Id { get; set; }
            public string Locale { get; set; }
            public ulong LastUpdate { get; set; }
            public int Answer { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Id).NotEmpty();
                RuleFor(c => c.Locale).NotEmpty();
                RuleFor(c => c.LastUpdate).NotEmpty();
                RuleFor(c => c.Answer).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<ChallengeResponse>>
        {

            private readonly IValidator<Command> _validator;
            private readonly IChallengeClient _challengeClient;

            public Handler(IValidator<Command> validator, IChallengeClient gameFailChallengeClient)
            {
                _validator = validator;
                _challengeClient = gameFailChallengeClient;
            }

            public async Task<Result<ChallengeResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var ValidationResult = _validator.Validate(request);

                if (!ValidationResult.IsValid)
                {
                    return Result.Fail<ChallengeResponse>(new Error("AuthApi.Challenge", new Error("Wrong parameters")));
                }

                var ChallengeRequest = request.Adapt<ChallengeRequest>();

                var ChallengeResult = await _challengeClient.CompleteChallenge(ChallengeRequest);

                if (ChallengeResult.IsSuccess)
                    return Result.Ok(ChallengeResult.Value);

                return Result.Fail<ChallengeResponse>(ChallengeResult.Errors);
            }
        }
    }

    public class ChallengeEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("auth/challenge", async (ChallengeRequest request, ISender sender) =>
            {
                var Command = request.Adapt<Challenge.Command>();

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
