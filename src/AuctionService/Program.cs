using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

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

    // Configuring MassTransit to use RabbitMQ as the messaging transport
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

try
{
    DbInitializer.InitDb(app);
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}

app.Run();
