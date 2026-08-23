using JobFinders.BLL.Interfaces;
using JobFinders.BLL.Models;
using JobFinders.BLL.Services;
using JobFinders.Data;
using JobFinders.Server.Configuration;

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;

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
builder.Services.AddScoped<IJobParser, JobParser>();
builder.Services.AddScoped<ITransliterator, Transliterator>();
builder.Services.AddSingleton<IPageObserver, PageObserver>();

var connectionString = builder.Configuration["JobFinderDB"];
builder.Services.AddDbContext<JobFinderDbContext>(options => options.UseSqlite(connectionString));

builder.Services.Configure<List<JobFinderSetting>>(builder.Configuration.GetSection("JobFinderSettings"));

var app = builder.Build();

Directory.CreateDirectory("../DB");

using var scope = app.Services.CreateAsyncScope();
var dbContext = scope.ServiceProvider.GetService<JobFinderDbContext>();

if (dbContext is null)
{
    throw new InvalidOperationException(nameof(dbContext) + $" - DbContext is null.");
}

await dbContext.Database.MigrateAsync();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();