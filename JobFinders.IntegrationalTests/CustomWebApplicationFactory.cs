using JobFinders.Domain.Interfaces;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Moq;
using Xunit;

namespace JobFinders.IntegrationalTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public Mock<IEmailSender> EmailSenderMock { get; } = new();
        public Mock<IJobFinderManager> JobFinderManagerMock { get; } = new();

        private readonly string _dbPath;

        public string DbPath => _dbPath;

        public CustomWebApplicationFactory()
        {
            _dbPath = Path.Combine(Path.GetTempPath(), $"jobfinders_test_{Guid.NewGuid():N}.db");

            Environment.SetEnvironmentVariable("JobFinderDB", $"Data Source={_dbPath}");
            Environment.SetEnvironmentVariable("JwtSecret", "integration-test-secret-key-that-is-long-enough-1234");
            Environment.SetEnvironmentVariable("JwtAudience", "jobfinder-test");
            Environment.SetEnvironmentVariable("JwtIssuer", "jobfinder-test");
            Environment.SetEnvironmentVariable("JwtExpiryMinutes", "60");
            Environment.SetEnvironmentVariable("Email", "recipient@example.com");
        }

        public new HttpClient CreateClient()
        {
            return base.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(EmailSenderMock.Object);
                services.AddSingleton(JobFinderManagerMock.Object);
            });
        }

        public Task InitializeAsync() => Task.CompletedTask;

        Task IAsyncLifetime.DisposeAsync()
        {
            Dispose();

            try
            {
                if (File.Exists(_dbPath))
                {
                    File.Delete(_dbPath);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }

            return Task.CompletedTask;
        }
    }
}