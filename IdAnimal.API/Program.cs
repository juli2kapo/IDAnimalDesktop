using System.Text;
using IdAnimal.API.Data;
using IdAnimal.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICloudStorageService, CloudinaryService>();

var app = builder.Build();

// --- 🟢 NEW LOGGING MIDDLEWARE STARTS HERE ---
// This must be the very first thing in the pipeline to catch everything
app.Use(async (context, next) =>
{
    // 1. Get the Logger
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();

    // 2. Log the Incoming Request
    var method = context.Request.Method;
    var path = context.Request.Path;
    logger.LogInformation($"➡️ Incoming Request: {method} {path}");

    // 3. Call the next middleware in the pipeline
    await next();

    // 4. Log the Outgoing Response
    var statusCode = context.Response.StatusCode;

    if (statusCode >= 400)
    {
        logger.LogWarning($"⬅️ Server Response: {statusCode} (Error/Bad Request) for {method} {path}");
    }
    else
    {
        logger.LogInformation($"⬅️ Server Response: {statusCode} (Success) for {method} {path}");
    }
});
// --- 🔴 LOGGING MIDDLEWARE ENDS HERE ---

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<AppDbContext>();

    try
    {
        var dbPath = Path.Combine(AppContext.BaseDirectory, "IdAnimal.db");
        
        if (File.Exists(dbPath))
        {
             logger.LogInformation($"✅ Database file found at: {dbPath}");
        }
        else
        {
             logger.LogWarning($"⚠️ Database file NOT found at: {dbPath}. Creating it now...");
             context.Database.Migrate(); 
        }

        logger.LogInformation("🔄 Attempting to apply migrations...");
        
        
        logger.LogInformation("✅ Database migration applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "❌ FATAL ERROR: Could not create or migrate the database.");
    }
}

app.Run();