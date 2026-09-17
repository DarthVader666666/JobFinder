using System.Net;
using System.Net.Http.Json;
using System.Text;

using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;

using Microsoft.Extensions.DependencyInjection;

using Moq;
using Xunit;

namespace JobFinders.IntegrationalTests
{
    public class JobsControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public JobsControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task GetJobs_WhenRequestIsValid_ReturnsJobsGrouped()
        {
            _factory.JobFinderManagerMock
                .Setup(m => m.ProcessAsync(It.IsAny<JobFinderSetting?>(), It.IsAny<JobsQuery?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((JobFinderSetting? setting, JobsQuery? query, CancellationToken _) => new[]
                {
                    new Job { Source = setting?.Source, Title = "Backend Developer", Link = $"https://jobs.example/{setting?.Source?.ToLower()}/1", Company = "Acme" },
                    new Job { Source = setting?.Source, Title = "Frontend Developer", Link = $"https://jobs.example/{setting?.Source?.ToLower()}/2", Company = "Beta" }
                });

            var response = await PostJobsAsync(new
            {
                sources = new[] { "RabotaBy", "PracaBy" },
                speciality = "developer",
                location = "minsk"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await ReadResponseAsync(response);
            Assert.NotNull(content);
            Assert.NotNull(content!.JobGroups);
            var jobs = content.JobGroups!.SelectMany(group => group).ToArray();
            Assert.Equal(4, jobs.Length);
            Assert.DoesNotContain(jobs, job => string.IsNullOrEmpty(job.Title));
            Assert.Contains(jobs, job => job.Title == "Backend Developer");
            Assert.Contains(jobs, job => job.Title == "Frontend Developer");
            Assert.False(content.HasMoreJobs);
        }

        [Fact]
        public async Task GetJobs_WhenBodyIsNull_ReturnsBadRequest()
        {
            _factory.JobFinderManagerMock
                .Setup(m => m.ProcessAsync(It.IsAny<JobFinderSetting?>(), It.IsAny<JobsQuery?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Job>());

            var response = await PostJobsAsync((string?)null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetJobs_WhenSpecialityIsMissing_ReturnsBadRequest()
        {
            var response = await PostJobsAsync(new
            {
                sources = new[] { "RabotaBy" },
                location = "minsk"
            });

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetJobs_WhenNoSources_ReturnsEmptyJobGroups()
        {
            _factory.JobFinderManagerMock
                .Setup(m => m.ProcessAsync(It.IsAny<JobFinderSetting?>(), It.IsAny<JobsQuery?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<Job>());

            var response = await PostJobsAsync(new
            {
                sources = Array.Empty<string>(),
                speciality = "developer",
                location = "minsk"
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await ReadResponseAsync(response);
            Assert.NotNull(content);
            Assert.NotNull(content!.JobGroups);
            Assert.Empty(content.JobGroups!);
            Assert.False(content.HasMoreJobs);
        }

        [Fact]
        public async Task GetJobs_WhenSameQuerySentTwice_ServesSecondRequestFromCache()
        {
            _factory.JobFinderManagerMock
                .Setup(m => m.ProcessAsync(It.IsAny<JobFinderSetting?>(), It.IsAny<JobsQuery?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[]
                {
                    new Job { Source = "RabotaBy", Title = "Cached Job", Link = "https://jobs.example/1", Company = "Acme" }
                });

            var body = new
            {
                sources = new[] { "RabotaBy" },
                speciality = "developer",
                location = "minsk-cache"
            };

            var first = await PostJobsAsync(body);
            var second = await PostJobsAsync(body);

            Assert.Equal(HttpStatusCode.OK, first.StatusCode);
            Assert.Equal(HttpStatusCode.OK, second.StatusCode);

            var firstContent = await ReadResponseAsync(first);
            var secondContent = await ReadResponseAsync(second);
            Assert.NotNull(firstContent);
            Assert.NotNull(secondContent);
            Assert.NotNull(firstContent!.JobGroups);
            Assert.NotNull(secondContent!.JobGroups);
            Assert.Single(firstContent.JobGroups!.SelectMany(g => g));
            Assert.Single(secondContent.JobGroups!.SelectMany(g => g));

            _factory.JobFinderManagerMock.Verify(
                m => m.ProcessAsync(It.IsAny<JobFinderSetting?>(), It.IsAny<JobsQuery?>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task GetJobs_WhenMoreJobsRequested_CombinesCachedAndNewJobs()
        {
            var observer = _factory.Services.GetRequiredService<IPageObserver>();
            int callCount = 0;

            _factory.JobFinderManagerMock
                .Setup(m => m.ProcessAsync(It.IsAny<JobFinderSetting?>(), It.IsAny<JobsQuery?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() =>
                {
                    callCount++;

                    observer.UpdateCounterAsync(
                        new PageCounterQuery("RabotaBy", "DEVELOPER", "MINSK", 0, true)).GetAwaiter().GetResult();

                    return callCount == 1
                        ? [new Job { Source = "RabotaBy", Title = "FirstPage", Link = "https://jobs.example/first", Company = "Acme" }]
                        : [new Job { Source = "RabotaBy", Title = "SecondPage", Link = "https://jobs.example/second", Company = "Acme" }];
                });

            var firstResponse = await PostJobsAsync(new
            {
                sources = new[] { "RabotaBy" },
                speciality = "developer",
                location = "minsk-more",
                moreJobs = false
            });

            Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);
            var firstContent = await ReadResponseAsync(firstResponse);
            Assert.NotNull(firstContent);
            Assert.True(firstContent!.HasMoreJobs);
            Assert.Contains(
                firstContent.JobGroups!.SelectMany(g => g),
                job => job.Title == "FirstPage");

            var secondResponse = await PostJobsAsync(new
            {
                sources = new[] { "RabotaBy" },
                speciality = "developer",
                location = "minsk-more",
                moreJobs = true
            });

            Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);
            var secondContent = await ReadResponseAsync(secondResponse);
            Assert.NotNull(secondContent);

            var secondJobs = secondContent!.JobGroups!.SelectMany(g => g).ToArray();
            Assert.Contains(secondJobs, job => job.Title == "FirstPage");
            Assert.Contains(secondJobs, job => job.Title == "SecondPage");
            Assert.True(secondContent.HasMoreJobs);
        }

        private Task<HttpResponseMessage> PostJobsAsync(object? body)
        {
            return _client.PostAsJsonAsync("/Jobs/GetJobs", body);
        }

        private Task<HttpResponseMessage> PostJobsAsync(string? rawJson)
        {
            var content = new StringContent(rawJson ?? "null", Encoding.UTF8, "application/json");
            return _client.PostAsync("/Jobs/GetJobs", content);
        }

        private static async Task<JobsResponseLite?> ReadResponseAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadFromJsonAsync<JobsResponseLite>();
        }

        private sealed class JobsResponseLite
        {
            public Job[][]? JobGroups { get; set; }
            public bool HasMoreJobs { get; set; }
        }
    }
}