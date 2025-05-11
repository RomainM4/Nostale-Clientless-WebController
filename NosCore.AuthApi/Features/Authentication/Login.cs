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
    public static class Login
    {
        public class Command : IRequest<Result<LoginResponse>>
        {
            public string Blackbox { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string Locale { get; set; } = string.Empty;
            public string InstallationId { get; set; } = string.Empty;

        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Blackbox).NotEmpty();
                RuleFor(c => c.Email).NotEmpty();
                RuleFor(c => c.Password).NotEmpty();
                RuleFor(c => c.Locale).NotEmpty();
                RuleFor(c => c.InstallationId).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<LoginResponse>>
        {

            private readonly IValidator<Command> _validator;
            private readonly IAuthenticationClient _authClient;
            private readonly IChallengeClient _challengeClient;

            public Handler(IValidator<Command> validator, IAuthenticationClient authClient, IChallengeClient challengeClient)
            {
                _validator = validator;
                _authClient = authClient;
                _challengeClient = challengeClient;
            }

            public async Task<Result<LoginResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var ValidationResult = _validator.Validate(request);

                if (!ValidationResult.IsValid)
                {
                    return Result.Fail<LoginResponse>(new Error("AuthApi.Login", new Error("Wrong parameters")));
                }

                var LoginRequest = request.Adapt<LoginRequest>();

                var LoginResult = await _authClient.GetLoginToken(LoginRequest);

                if(LoginResult.IsSuccess)
                {
                    if (LoginResult.Value.IsChallenge)
                    {
                        ChallengeRequest? ChallengeRequest = new ChallengeRequest
                        {
                            Id = LoginResult.Value.ChallengeId,
                            Locale = LoginResult.Value.Locale
                        };

                        var ChallengeResult = await _challengeClient.GetChallenge(ChallengeRequest);

                        if (ChallengeResult.IsSuccess)
                        {
                            ChallengeRequest.LastUpdate = ChallengeResult.Value.lastUpdated;

                            var TextResult = await _challengeClient.GetTextImage(ChallengeRequest);
                            var IconsResult = await _challengeClient.GetIconsImage(ChallengeRequest);
                            var TargetIconResult = await _challengeClient.GetTargetIconImage(ChallengeRequest);

                            if(TextResult.IsSuccess && IconsResult.IsSuccess && TargetIconResult.IsSuccess)
                            {
                                LoginResult.Value.ChallengeTextImage        = TextResult.Value;
                                LoginResult.Value.ChallengeIconsImage       = IconsResult.Value;
                                LoginResult.Value.ChallengeTargetIconImage  = TargetIconResult.Value;

                                return Result.Ok(LoginResult.Value);
                            }

                            return Result.Fail<LoginResponse>(new Error("AuthApi.Login", new Error("Challenge Http requests failed")));
                        }
                    }

                    return Result.Ok(LoginResult.Value);
                }

                return Result.Fail<LoginResponse>(LoginResult.Errors);
            }
        }
    }

    public class LoginEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("auth/login", async (LoginRequest request, ISender sender) =>
            {
                var Command = request.Adapt<Login.Command>();

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
