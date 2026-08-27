using JobFinders.Api.Configuration;

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;

using JobFinders.Application.Services;

using JobFinders.DAL;
using JobFinders.DAL.Repositories;

using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;
using JobFinders.Domain.Entities;

using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();

builder.Services.AddCors(options => options.AddPolicy("AllowClient",
    new CorsPolicyBuilder()
    .WithOrigins(origins ?? [])
    .AllowAnyHeader().AllowAnyMethod().Build()));

builder.Services.AddMemoryCache();
builder.Services.ConfigureAutomapper();

var jwtSettings = new JwtSettings
{
    Secret = builder.Configuration["JwtSecret"],
    Audience = builder.Configuration["JwtAudience"],
    Issuer = builder.Configuration["JwtIssuer"],
    ExpiryMinutes = int.Parse(builder.Configuration["JwtExpiryMinutes"] ?? throw new InvalidOperationException("JwtExpiryMinutes not configured")),
};

var key = Encoding.ASCII.GetBytes(jwtSettings?.Secret ?? throw new InvalidOperationException("JwtSecret not configured"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = jwtSettings.Audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<IJobFinderManager, JobFinderManager>();
builder.Services.AddScoped<IEmailSender, AzureEmailSender>();
builder.Services.AddScoped<IHtmlLoader, HtmlLoader>();
builder.Services.AddScoped<ITransliterator, Transliterator>();
builder.Services.AddSingleton<IPageObserver, PageObserver>();
builder.Services.AddScoped<IRepository<User>, Repository<User>>();
builder.Services.AddScoped<IRepository<Role>, Repository<Role>>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.Configure<List<JobFinderSetting>>(builder.Configuration.GetSection("JobFinderSettings"));

var connectionString = builder.Configuration["JobFinderDB"];
builder.Services.AddDbContext<JobFinderDbContext>(options => options.UseSqlite(connectionString));

Directory.CreateDirectory("../DB");

var app = builder.Build();

using var scope = app.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetService<JobFinderDbContext>();

if (dbContext is null)
{
    throw new InvalidOperationException(nameof(dbContext) + $" - DbContext is null.");
}

try
{
    await dbContext.Database.MigrateAsync();
}
catch (Exception ex)
{
    Console.ForegroundColor = ex.Message switch
    {
        var m when m.Contains("warning") => ConsoleColor.Yellow,
        var m when m.Contains("error") => ConsoleColor.Red,
        _ => ConsoleColor.Gray,
    };

    Console.WriteLine(ex.Message);

    return;
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();