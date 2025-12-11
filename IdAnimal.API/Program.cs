using System.Text;
using IdAnimal.API.Data;
using IdAnimal.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Dotmim.Sync;
using Dotmim.Sync.Sqlite;
using Dotmim.Sync.Web.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddLogging();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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

var syncSetup = new SyncSetup(
    "Establishments", 
    "Cattle", 
    "CattleImages",      // Syncs the metadata (paths/IDs), NOT the actual files
    "CattleFullImages", 
    "CattleVideos"
);

syncSetup.Filters.Add("Establishments", "UserId");
syncSetup.Filters.Add("Cattle", "UserId");
syncSetup.Filters.Add("CattleImages", "UserId");
syncSetup.Filters.Add("CattleFullImages", "UserId");
syncSetup.Filters.Add("CattleVideos", "UserId");

builder.Services.AddSyncServer<SqliteSyncProvider>(connectionString, syncSetup);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICloudStorageService, CloudinaryService>();
builder.Services.AddSingleton<ISnoutAnalysisService, SnoutAnalysisService>();

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
});

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
        }
        context.Database.Migrate(); 

        logger.LogInformation("🔄 Attempting to apply migrations...");
        
        
        logger.LogInformation("✅ Database migration applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "❌ FATAL ERROR: Could not create or migrate the database.");
    }
}

app.Run();