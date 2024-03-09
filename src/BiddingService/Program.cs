using BiddingService;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using MongoDB.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configuring MassTransit with services in the application
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    // Set the endpoint name formatter to kebab case with "bids" prefix
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));

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

// Adding authentication services to the service collection.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

    // Configuring JwtBearer authentication options.
    .AddJwtBearer(options =>
    {
        // Setting the authority to the Identity Service URL retrieved from configuration.
        options.Authority = builder.Configuration["IdentityServiceUrl"];

        // Disabling HTTPS metadata validation (useful for development).
        options.RequireHttpsMetadata = false;

        // Disabling audience validation.
        options.TokenValidationParameters.ValidateAudience = false;

        // Setting the type of claim to be used as the user's name.
        options.TokenValidationParameters.NameClaimType = "username";
    });

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHostedService<CheckAuctionFinished>();
builder.Services.AddScoped<GrpcAuctionClient>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

await DB.InitAsync("BidDb", MongoClientSettings
    .FromConnectionString(builder.Configuration.GetConnectionString("BidDbConnection")));

app.Run();
