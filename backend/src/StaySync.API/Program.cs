using Microsoft.EntityFrameworkCore;
using StaySync.API.Middleware;
using StaySync.Application;
using StaySync.Domain.Entities;
using StaySync.Domain.Enums;
using StaySync.Infrastructure;
using StaySync.Infrastructure.Identity;
using StaySync.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await SeedDevDataAsync(app);
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<StaySync.Infrastructure.Persistence.AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task SeedDevDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    await SeedUserAsync(db, "admin@staysync.dev", "Admin1234!", UserRole.SuperAdmin, null);
    await SeedUserAsync(db, "manager@staysync.dev", "Manager1234!", UserRole.PropertyManager, "Demo Property Manager");
    await SeedUserAsync(db, "manager2@staysync.dev", "Manager1234!", UserRole.PropertyManager, "Second Property Manager");
}

static async Task SeedUserAsync(AppDbContext db, string email, string password, UserRole role, string? displayName)
{
    if (await db.Users.AnyAsync(u => u.Email == email))
        return;

    var user = new User
    {
        Email = email,
        PasswordHash = PasswordHasher.Hash(password),
        Role = role
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    if (role == UserRole.PropertyManager && displayName is not null)
    {
        db.PropertyManagers.Add(new PropertyManager { UserId = user.Id, DisplayName = displayName });
        await db.SaveChangesAsync();
    }
}
