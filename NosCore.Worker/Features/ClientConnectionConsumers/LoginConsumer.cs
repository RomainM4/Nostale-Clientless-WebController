using Contracts.Responses;
using MassTransit;
using NostaleSdk.Configs.Interfaces;
using NostaleSdk.Configs;
using Polly;
using Polly.Timeout;
using Contracts.Requests;
using NosCore.Worker.Services.Client;
using NostaleSdk.Packet;

namespace NosCore.Worker.Features.ClientConnectionConsumers
{
    public class LoginConsumer : IConsumer<LoginClientConnectionRequest>
    {
        private readonly IClientControllerService _controller;

        public LoginConsumer(IClientControllerService client)
        {
            _controller = client;
        }

        public async Task Consume(ConsumeContext<LoginClientConnectionRequest> context)
        {
            IConnectionConfig Config;
            LoginClientConnectionRequest? Request = context.Message;

            if (_controller.IsInWorld())
            {
                await context.RespondAsync(new ClientConnectionExceptionResponse { Error = "Error client is already connected" });
                return;
            }

            switch (Request.ServerType)
            {
                case ServerType.Alzanor:
                    Config = new AlzanorConfig(Request.ProxyType, Request.Username, Request.Password, Request.Host, Request.Port, Request.CanalSlot);
                    break;
                case ServerType.Cosmos:
                    Config = new CosmosConfig(Request.ProxyType, Request.Username, Request.Password, Request.Host, Request.Port, Request.CanalSlot);
                    break;
                case ServerType.Dragonveil:
                    Config = new DragonveilConfig(Request.ProxyType, Request.Username, Request.Password, Request.Host, Request.Port, Request.CanalSlot);
                    break;
                case ServerType.Jotunheim:
                    Config = new JotunheimConfig(Request.ProxyType, Request.Username, Request.Password, Request.Host, Request.Port, Request.CanalSlot);
                    break;
                case ServerType.Valehir:
                    Config = new ValehirConfig(Request.ProxyType, Request.Username, Request.Password, Request.Host, Request.Port, Request.CanalSlot);
                    break;
                default:
                    Config = new CosmosConfig(Request.ProxyType, Request.Username, Request.Password, Request.Host, Request.Port, Request.CanalSlot);
                    break;
            }

            var Pipeline = new ResiliencePipelineBuilder<List<NoscoreObservedPacket>>()
                .AddTimeout(new TimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(120),
                    OnTimeout = async args =>
                    {
                        await context.RespondAsync(new TimeoutExceptionResponse
                        {
                            Error = "Timeout during login"
                        });
                    }
                }).Build();


            var Token = Request.Token;
            var AccountName = Request.AccountName;
            var CharacterSlot = Request.CharacterSlot;

            var Packets = await Pipeline.ExecuteAsync(async token => await _controller.ConnectToServer(Config, Token, AccountName, CharacterSlot, token));

            if (Packets == null)
            {
                await context.RespondAsync(new ClientConnectionExceptionResponse
                {
                    Error = "Gameforge login failed, verify Token"
                });

                return;
            }

            await context.RespondAsync(new LoginClientConnectionResponse
            {
                Packets = Packets
            }); ;
        }
    }
}
