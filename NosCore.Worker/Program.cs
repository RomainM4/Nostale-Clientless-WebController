using MassTransit;
using NosCore.Worker;
using NosCore.Worker.Features.ClientConnectionConsumers;
using NosCore.Worker.Features.NosbazarConsumers;
using NosCore.Worker.Services.Client;
using NosCore.Worker.Services.NosData;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddSingleton<IClientControllerService, ClientControllerService>();
builder.Services.AddSingleton<INosDataService, NosDataService>();
builder.Services.AddHostedService<Worker>();
builder.Services.AddMassTransit(busConfig =>
{
    busConfig.SetKebabCaseEndpointNameFormatter();

    busConfig.AddConsumer<SearchConsumer>();
    busConfig.AddConsumer<OpenConsumer>();

    busConfig.AddConsumer<LoginConsumer>();
    busConfig.AddConsumer<DisconnectConsumer>();

    busConfig.AddConsumer<ObserverConsumer>();

    busConfig.UsingRabbitMq((context, configurator) =>
    {
        configurator.Host(new Uri(builder.Configuration["MessageBroker:Host"]!), h =>
        {
            h.Username(builder.Configuration["MessageBroker:Username"]!);
            h.Password(builder.Configuration["MessageBroker:Password"]!);
        });

        configurator.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();
