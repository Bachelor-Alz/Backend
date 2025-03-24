using HealthDevice.Data;
using Microsoft.EntityFrameworkCore;
using HealthDevice.Controllers;
using HealthDevice.DTO;
using HealthDevice.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<AIController>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddIdentity<User, IdentityRole>(options =>
        options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
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