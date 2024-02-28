using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionServiceHttpClient>().AddPolicyHandler(GetPolicy());

// Configuring MassTransit with services in the application
builder.Services.AddMassTransit(x =>
{
    // Add consumers from the namespace containing AuctionCreatedConsumer
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    // Set the endpoint name formatter to kebab case with "search" prefix
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search"));

    // Configure MassTransit to use RabbitMQ
    x.UsingRabbitMq((context, cfg) =>
    {
        // Configure RabbitMQ host address
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });

        // Configure a specific receive endpoint for handling AuctionCreated messages
        cfg.ReceiveEndpoint("search-auction-created", e =>
        {
            // Use message retry with an interval of 5 seconds
            e.UseMessageRetry(r => r.Interval(5, 5));

            // Configure the consumer for handling AuctionCreated messages
            e.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });

        // Configure other endpoints based on conventions
        cfg.ConfigureEndpoints(context);
    });
});


var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

// Registering the database initialization within the ApplicationStarted event
app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }

});


app.Run();

// Policy for retrying HTTP requests on transient errors or NotFound (404) status codes, with indefinite retries every 3 seconds.
static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));