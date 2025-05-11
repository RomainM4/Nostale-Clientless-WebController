using FluentResults;
using FluentValidation;
using Mapster;
using MediatR;
using Carter;
using Contracts.Responses.Nosbazar;
using Contracts.Requests.Nosbazar;
using NostaleSdk.Nosbazar;
using NosCore.ClientApi.Services.Nosbazar;

namespace NosCore.ClientApi.Features.Nosbazar
{
    public static class Open
    {
        public class Command : IRequest<Result<OpenResponse>>
        {
            public int Id { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {

            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<OpenResponse>>
        {

            private readonly IValidator<Command> _validator;
            private readonly NosbazarService _nosbazarService;

            public Handler(IValidator<Command> validator, NosbazarService nosbazarService)
            {
                _validator = validator;
                _nosbazarService = nosbazarService;
            }

            public async Task<Result<OpenResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var ValidationResult = _validator.Validate(request);

                if (!ValidationResult.IsValid)
                {
                    return Result.Fail<OpenResponse>(new Error("ClientApi.Nosbazar.Open", new Error("Wrong parameters")));
                }

                var OpenRequest = request.Adapt<OpenRequest>();

                var ResponseResult = await _nosbazarService.OpenAsync(OpenRequest);

                if (ResponseResult.IsSuccess)
                    return Result.Ok(ResponseResult.Value);

                return Result.Fail<OpenResponse>(ResponseResult.Errors);
            }
        }
    }
    public class OpenEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("client/nosbazar/open", async (OpenRequest request, ISender sender) =>
            {
                var Command = request.Adapt<Open.Command>();

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
