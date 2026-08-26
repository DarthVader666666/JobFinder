using JobFinders.Api.Configuration;

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;

using JobFinders.Application.Services;

using JobFinders.DAL;
using JobFinders.DAL.Repositories;

using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;
using JobFinders.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();

builder.Services.AddCors(options => options.AddPolicy("AllowClient",
    new CorsPolicyBuilder()
    .WithOrigins(origins ?? [])
    .AllowAnyHeader().AllowAnyMethod().Build()));

builder.Services.AddMemoryCache();
builder.Services.ConfigureAutomapper();

builder.Services.AddScoped<IJobFinderManager, JobFinderManager>();
builder.Services.AddScoped<IEmailSender, AzureEmailSender>();
builder.Services.AddScoped<IHtmlLoader, HtmlLoader>();
builder.Services.AddScoped<ITransliterator, Transliterator>();
builder.Services.AddSingleton<IPageObserver, PageObserver>();

var connectionString = builder.Configuration["JobFinderDB"];
builder.Services.AddDbContext<JobFinderDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddScoped<IRepository<User>, Repository<User>>();
builder.Services.AddScoped<IRepository<Role>, Repository<Role>>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.Configure<List<JobFinderSetting>>(builder.Configuration.GetSection("JobFinderSettings"));

var app = builder.Build();

Directory.CreateDirectory("../DB");

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();