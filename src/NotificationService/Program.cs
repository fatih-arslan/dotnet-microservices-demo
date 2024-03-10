using MassTransit;
using NotificationService;

var builder = WebApplication.CreateBuilder(args);

// Configuring MassTransit with services in the application
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    // Set the endpoint name formatter to kebab case with "bids" prefix
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("nt", false));

    // Configuring MassTransit to use RabbitMQ as the messaging transport
    x.UsingRabbitMq((context, cfg) =>
    {
        // Configure RabbitMQ host address
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });

        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.AddSignalR();

var app = builder.Build();

app.MapHub<NotificationHub>("/notifications");

app.Run();
