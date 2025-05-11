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
    public static class Accounts
    {
        public class Command : IRequest<Result<AccountsResponse>>
        {
            public required string Token { get; set; }
            public required string InstallationId { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(c => c.Token).NotEmpty();
                RuleFor(c => c.InstallationId).NotEmpty();
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<AccountsResponse>>
        {

            private readonly IValidator<Command> _validator;
            private readonly IAuthenticationClient _authClient;

            public Handler(IValidator<Command> validator, IAuthenticationClient authClient)
            {
                _validator = validator;
                _authClient = authClient;
            }

            public async Task<Result<AccountsResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var ValidationResult = _validator.Validate(request);

                if (!ValidationResult.IsValid)
                {
                    return Result.Fail<AccountsResponse>(new Error("AuthApi.Accounts", new Error("Wrong parameters")));
                }

                var AccountRequest = request.Adapt<AccountsRequest>();

                var AccountsResult = await _authClient.GetAccounts(AccountRequest);

                if (AccountsResult.IsSuccess)
                    return Result.Ok(AccountsResult.Value);

                return Result.Fail<AccountsResponse>(AccountsResult.Errors);
            }
        }
    }

    public class AccountsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("auth/accounts", async (AccountsRequest request, ISender sender) =>
            {
                var Command = request.Adapt<Accounts.Command>();

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
