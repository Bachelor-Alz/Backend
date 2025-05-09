using HealthDevice.Data;
using HealthDevice.Controllers;
using HealthDevice.Models;
using HealthDevice.Services;
using HealthDevice.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddScoped<UserController>(); 
builder.Services.AddScoped<ArduinoController>();
builder.Services.AddScoped<HealthController>();
builder.Services.AddScoped<AiController>();
builder.Services.AddScoped<TestController>();
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
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<GeoService>();

builder.Services.AddControllers();

// Register generic repository and factory
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ApplicationDbContext>();
builder.Services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

// Register HealthService with its interface
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IGeoService, GeoService>();
builder.Services.AddScoped<IArduinoService, ArduinoService>();
builder.Services.AddScoped<IAIService, AiService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IGetHealthData, GetHealthDataService>();
builder.Services.AddScoped<ITimeZoneService, TimeZoneService>();

builder.Services.ConfigureApplicationCookie();

builder.Services.AddOpenApi();
builder.Services.AddJwtAuthentication(
    "api.healthdevice.com",
    "user.healthdevice.com",
    "UGVuaXNQZW5pc1BlbmlzUGVuaXNQZW5pc1BlbmlzUGVuaXNQZW5pc1BlbmlzUGVuaXNQZW5pc1Blbmlz"
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
builder.Services.AddScoped<TimedGPSService>();

builder.Services.AddHealthChecks()
    .AddCheck<EnvVarHealthCheck>("Environment Variables")
    .AddCheck<DbHealthCheck>("Database");


var app = builder.Build();

app.UseHealthChecks("/health");
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync(); // Applies any pending migrations
    // Call MakeTestUserAsync with the service provider
    if (Environment.GetEnvironmentVariable("IS_TESTING") == "true")
    {
        await TestUserConfig.MakeTestUserAsync(scope.ServiceProvider);
    }
}

app.Run();
