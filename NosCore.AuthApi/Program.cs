using Carter;
using FluentValidation;
using NosCore.AuthApi.HttpClients.Interfaces;
using NosCore.AuthApi.HttpClients;

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

builder.Services.AddHttpClient<IAuthenticationClient, AuthenticationClient>();
builder.Services.AddHttpClient<IChallengeClient, ChallengeClient>();
builder.Services.AddHttpClient<IVersionClient, VersionClient>();
builder.Services.AddHttpClient<IDateTimeClient, DateTimeClient>();


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
