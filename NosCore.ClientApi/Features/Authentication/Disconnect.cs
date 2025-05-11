using FluentResults;
using FluentValidation;
using Mapster;
using MediatR;
using Carter;
using Contracts.Responses.Nosbazar;
using Contracts.Requests.Nosbazar;
using NostaleSdk.Nosbazar;
using NosCore.ClientApi.Services.Nosbazar;
using NostaleSdk.Configs.Interfaces;
using Contracts.Responses;
using Contracts.Requests;
using NosCore.ClientApi.Services.Authentication;

namespace NosCore.ClientApi.Features.Authentication
{
    public static class Disconnect
    {
        public class Command : IRequest<Result>
        {

        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {

            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result>
        {

            private readonly IValidator<Command> _validator;
            private readonly AuthenticationService _authService;

            public Handler(IValidator<Command> validator, AuthenticationService authService)
            {
                _validator = validator;
                _authService = authService;
            }

            public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
            {
                var ValidationResult = _validator.Validate(request);

                if (!ValidationResult.IsValid)
                {
                    return Result.Fail(new Error("ClientApi.Authentication.Disconnect", new Error("Wrong parameters")));
                }

                var LoginRequest = request.Adapt<DisconnectClientConnectionRequest>();

                await _authService.DisconnectAsync();

                return Result.Ok();
            }
        }
    }
    public class DisconnectEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("client/auth/disconnect", async (DisconnectClientConnectionRequest request, ISender sender) =>
            {
                var Command = request.Adapt<Disconnect.Command>();

                var Result = await sender.Send(Command);

                if (Result.IsFailed)
                {
                    return Results.BadRequest(Result.Errors);
                }

                return Results.Ok();
            });
        }
    }
}
