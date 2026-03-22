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

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await SeedDevDataAsync(app);
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static async Task SeedDevDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();

    if (await db.Users.AnyAsync())
        return;

    var adminUser = new User
    {
        Email = "admin@staysync.dev",
        PasswordHash = PasswordHasher.Hash("Admin1234!"),
        Role = UserRole.SuperAdmin
    };

    var pmUser = new User
    {
        Email = "manager@staysync.dev",
        PasswordHash = PasswordHasher.Hash("Manager1234!"),
        Role = UserRole.PropertyManager
    };

    db.Users.AddRange(adminUser, pmUser);
    await db.SaveChangesAsync();

    var propertyManager = new PropertyManager
    {
        UserId = pmUser.Id,
        DisplayName = "Demo Property Manager"
    };

    db.PropertyManagers.Add(propertyManager);
    await db.SaveChangesAsync();
}
