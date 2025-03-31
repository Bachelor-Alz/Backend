using HealthDevice.Data;
using HealthDevice.Controllers;
using HealthDevice.DTO;
using HealthDevice.Services;
using HealthDevice.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<AiController>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity services
builder.Services.AddIdentityCore<Elder>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddIdentityCore<Caregiver>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register UserManager and RoleManager for Elder
builder.Services.AddScoped<UserManager<Elder>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// Register UserManager and RoleManager for Caregiver
builder.Services.AddScoped<UserManager<Caregiver>>();
builder.Services.AddScoped<RoleManager<IdentityRole>>();

// Register UserService, ArduinoService, and HealthService
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ArduinoService>();
builder.Services.AddScoped<HealthService>();
builder.Services.AddScoped<AiService>();

builder.Services.AddRazorPages();

builder.Services.ConfigureApplicationCookie();

builder.Services.AddOpenApi();
builder.Services.AddJwtAuthentication(
    "api.healthdevice.com",
    "user.healthdevice.com",
    "Your_32_Character_Long_Secret_Key_Here"
);

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Caregiver", policy => policy.RequireClaim("Caregiver"));
    options.AddPolicy("Elder", policy => policy.RequireClaim("Elder"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

var requireAuthPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddAuthorizationBuilder()
    .SetDefaultPolicy(requireAuthPolicy);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()  // Log only Info, Warning, and Error
    .WriteTo.Console()  // Output logs to console
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddHostedService<TimedHostedService>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate(); // Applies any pending migrations
}

app.Run();