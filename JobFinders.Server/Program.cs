using JobFinders.BLL.Interfaces;
using JobFinders.BLL.Models;
using JobFinders.BLL.Services;
using JobFinders.Server.Middleware;

using Microsoft.AspNetCore.Cors.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();

builder.Services.AddCors(options => options.AddPolicy("AllowClient",
    new CorsPolicyBuilder()
    .WithOrigins(origins ?? [])
    .AllowAnyHeader().AllowAnyMethod().Build()));

builder.Services.AddScoped<IJobFinderManager, JobFinderManager>();
builder.Services.AddScoped<IEmailSender, AzureEmailSender>();
builder.Services.AddScoped<IHtmlLoader, HtmlLoader>();
builder.Services.AddScoped<IJobParser, JobParser>();
builder.Services.AddScoped<ITransliterator, Transliterator>();

builder.Services.Configure<List<JobFinderSetting>>(builder.Configuration.GetSection("JobFinderSettings"));

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();