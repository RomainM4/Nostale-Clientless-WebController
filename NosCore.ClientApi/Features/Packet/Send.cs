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
using NostaleSdk.Packet;
using Contracts.Requests;
using Contracts.Responses;
using Contracts.Auth;
using NosCore.ClientApi.Services.Packet;

namespace NosCore.ClientApi.Features.Packet
{
    public static class Send
    {
        public class Command : IRequest<Result<PacketObserverResponse>>
        {
            public Role role { get; set; } = Role.User;
            public List<NoscoreObservablePacket> Packets { get; set; } = new List<NoscoreObservablePacket>();
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
               
            }
        }

        internal sealed class Handler : IRequestHandler<Command, Result<PacketObserverResponse>>
        {

            private readonly IValidator<Command> _validator;
            private readonly PacketObserverService _packetLoggerService;

            public Handler(IValidator<Command> validator, PacketObserverService packetLoggerService)
            {
                _validator = validator;
                _packetLoggerService = packetLoggerService;
            }

            public async Task<Result<PacketObserverResponse>> Handle(Command request, CancellationToken cancellationToken)
            {
                var ValidationResult = _validator.Validate(request);

                if (!ValidationResult.IsValid)
                {
                    return Result.Fail<PacketObserverResponse>(new Error("ClientApi.Packet.Send", new Error("Wrong parameters")));
                }

                var ObserverRequest = request.Adapt<PacketObserverRequest>();

                var ObserverResult = await _packetLoggerService.SendAsync(ObserverRequest);

                if (ObserverResult.IsSuccess)
                {
                    //var CustomPacket = RcBlistCustomPacket.RcBlistParser.Parse(SearchResult.Value.RcBlistObservedPacket.ReceivePacket);

                    return Result.Ok(ObserverResult.Value);

                }

                return Result.Fail(ObserverResult.Errors);
            }
        }
    }

    public class AccountsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("client/packet/send", async (PacketObserverRequest request, ISender sender) =>
            {
                var Command = request.Adapt<Send.Command>();

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
