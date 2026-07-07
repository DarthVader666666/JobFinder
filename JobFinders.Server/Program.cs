using JobFinders.Data;
using JobFinders.Data.Entities;
using JobFinders.Data.Repositories;
using JobFinders.Server.Services;

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var origins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>();

builder.Services.AddCors(options => options.AddPolicy("AllowClient",
    new CorsPolicyBuilder()
    .WithOrigins(origins ?? [])
    .AllowAnyHeader().AllowAnyMethod().Build()));

builder.Services.AddDbContext<JobFindersDbContext>(opts => opts.UseInMemoryDatabase("JobFindersInMemoryDb"));

builder.Services.AddScoped<IRepository<JobFinder>, JobFinderRepository>();

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<JobFindersDbContext>();
JobFindersHelper.Seed(dbContext);

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();

app.UseCors();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();