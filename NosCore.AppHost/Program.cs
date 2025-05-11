var builder = DistributedApplication.CreateBuilder(args);

builder.AddContainer("rabbitmq", "rabbitmq", "management-alpine")
    .WithEnvironment("RABBITMQ_DEFAULT_USER", builder.Configuration["Rabbitmq:RABBITMQ_DEFAULT_USER"])
    .WithEnvironment("RABBITMQ_DEFAULT_PASS", builder.Configuration["Rabbitmq:RABBITMQ_DEFAULT_PASS"]);
    //.WithServiceBinding(15672, "http")
    //.WithServiceBinding(5672, "amqp");

var controllerApi = builder.AddProject<Projects.NosCore_ClientApi>("noscore-clientapi");
var authenticationApi = builder.AddProject<Projects.NosCore_GameAuthenticationApi>("noscore-authenticationapi");


builder.AddProject<Projects.NosCore_Worker>("noscore-worker");


builder.Build().Run();
