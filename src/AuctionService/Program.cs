using AuctionService;
using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AuctionDbContext>(
    options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Configuring MassTransit with services in the application
builder.Services.AddMassTransit(x =>
{

    // Adding Entity Framework Outbox for message reliability and persistence,
    // using AuctionDbContext as the database context
    x.AddEntityFrameworkOutbox<AuctionDbContext>(o =>
    {
        // Setting a delay for querying the outbox to avoid frequent checks
        o.QueryDelay = TimeSpan.FromSeconds(10);

        // Configuring to use PostgreSQL as the underlying storage mechanism
        o.UsePostgres();
        // UseBusOutBox enables the usage of the MassTransit outbox feature.
        o.UseBusOutbox();
    });

    // Adding consumers from the namespace containing AuctionFinishedConsumer
    x.AddConsumersFromNamespaceContaining<AuctionFinishedConsumer>();

    // Setting endpoint name formatter to kebab case for "auction" endpoints
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));

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

builder.Services.AddGrpc();

builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGrpcService<GrpcAuctionService>();

var retryPolicy = Policy
    .Handle<NpgsqlException>()
    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(10));

retryPolicy.ExecuteAndCapture(() => DbInitializer.InitDb(app));



app.Run();
