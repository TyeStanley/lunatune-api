using Microsoft.EntityFrameworkCore;
using Lunatune.Core.Interfaces;
using Lunatune.Infrastructure.Data;
using Lunatune.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Lunatune.Api.Middleware;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

var env = builder.Environment.EnvironmentName;
Console.WriteLine($"Running in {env} environment");

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins(
                "http://localhost:3000",
                "https://lunatune-app.vercel.app"
            )
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("Authorization", "Content-Type", "Accept")
            .AllowCredentials()
    );
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions => npgsqlOptions.MigrationsAssembly("Lunatune.Infrastructure")
    )
);

builder.Services.AddScoped<ISongService, SongService>();
builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISongLikeService, SongLikeService>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();

// If we're in production, use Auth0 authentication
if (builder.Environment.IsProduction())
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = configuration["Auth0:Domain"];
            options.Audience = configuration["Auth0:Audience"];
            options.RequireHttpsMetadata = true;
        });
    builder.Services.AddAuthorization();
}
// In development, add a dummy authentication scheme that always succeeds
else
{
    builder.Services.AddAuthentication("AllowAll")
        .AddScheme<AuthenticationSchemeOptions, AllowAllAuthenticationHandler>("AllowAll", null);
    builder.Services.AddAuthorizationBuilder()
        .AddPolicy("AllowAll", policy => policy.RequireAssertion(_ => true));
}

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = 429;

    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("per-user", opt =>
    {
        opt.PermitLimit = 20;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();

app.UseMiddleware<UserBlockingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.UseRateLimiter();
app.MapControllers();

app.Run();