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
    public static class Login
    {
        public class Command : IRequest<Result<LoginClientConnectionResponse>>
        {
            public string Token { get; set; }
            public string AccountName { get; set; }
            public int CharacterSlot { get; set; }
            public ServerType ServerType { get; set; }
            public int CanalSlot { get; set; }
            public ProxyType ProxyType { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Host { get; set; }
            public short Port { get; set; }


        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {

            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<LoginClientConnectionResponse>>
        {

            private readonly IValidator<Command> _validator;
            private readonly AuthenticationService _authService;

            public Handler(IValidator<Command> validator, AuthenticationService authService)
            {
                _validator = validator;
                _authService = authService;
            }

            public async Task<Result<LoginClientConnectionResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var ValidationResult = _validator.Validate(request);

                if (!ValidationResult.IsValid)
                {
                    return Result.Fail(new Error("ClientApi.Authentication.Login", new Error("Wrong parameters")));
                }

                var LoginRequest = request.Adapt<LoginClientConnectionRequest>();

                var ResponseResult = await _authService.ConnectToServerAsync(LoginRequest);

                if (ResponseResult.IsSuccess)
                    return Result.Ok(ResponseResult.Value);

                return Result.Fail(ResponseResult.Errors);
            }
        }
    }
    public class LoginEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("client/auth/login", async (LoginClientConnectionRequest request, ISender sender) =>
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
