using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

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


var app = builder.Build();

app.MapReverseProxy();

app.UseAuthentication();
app.UseAuthorization();

app.Run();
