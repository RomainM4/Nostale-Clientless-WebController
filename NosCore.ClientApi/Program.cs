using Carter;
using FluentValidation;
using MassTransit;
using NosCore.ClientApi.Services.Authentication;
using NosCore.ClientApi.Services.Nosbazar;
using NosCore.ClientApi.Services.Packet;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(Config => Config.RegisterServicesFromAssembly(assembly));
builder.Services.AddCarter();
builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddScoped<NosbazarService>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<PacketObserverService>();


//builder.Services.AddMediatR(config =>
//{
//    config.RegisterServicesFromAssemblies(assembly);
//});
//builder.Services.AddCarter();
//builder.Services.AddValidatorsFromAssembly(assembly);
builder.Services.AddMassTransit(busConfig =>
{
    busConfig.SetKebabCaseEndpointNameFormatter();

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


var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapCarter();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
