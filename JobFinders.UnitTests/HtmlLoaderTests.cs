using JobFinders.Application.Services;
using JobFinders.Domain.Interfaces;
using JobFinders.Domain.Models;

using Moq;
using Xunit;

namespace JobFinders.UnitTests
{
    public class HtmlLoaderTests
    {
        private readonly Mock<ITransliterator> _transliteratorMock;
        private readonly Mock<IPageObserver> _pageObserverMock;

        public HtmlLoaderTests()
        {
            _transliteratorMock = new Mock<ITransliterator>();
            _pageObserverMock = new Mock<IPageObserver>();
        }

        private HtmlLoader CreateLoader()
        {
            return new HtmlLoader(_transliteratorMock.Object, _pageObserverMock.Object);
        }

        [Fact]
        public async Task GetJobsAsync_WhenSettingIsNull_ThrowsArgumentNullException()
        {
            var loader = CreateLoader();

            await Assert.ThrowsAsync<ArgumentNullException>(() => loader.GetJobsAsync(null, new JobsQuery()));
        }

        [Fact]
        public async Task GetJobsAsync_WhenCounterHasNoNextPage_ReturnsEmptyResult()
        {
            _pageObserverMock
                .Setup(x => x.InitializeCounter(It.IsAny<PageCounterQuery>()))
                .Returns(new PageCounter("test") { HasNextPage = false });

            var loader = CreateLoader();

            var result = await loader.GetJobsAsync(new JobFinderSetting { Source = "test" }, new JobsQuery());

            Assert.Empty(result);
            _transliteratorMock.Verify(x => x.Transliterate(It.IsAny<string?>(), It.IsAny<JobFinderSetting?>()), Times.Never);
            _pageObserverMock.Verify(x => x.UpdateCounterAsync(It.IsAny<PageCounterQuery?>()), Times.Never);
        }

        [Fact]
        public async Task GetJobsAsync_WhenLoadFails_ReturnsSingleErrorJobWithBuiltUrl()
        {
            var setting = new JobFinderSetting
            {
                Source = "test",
                LinkTemplate = "bad! *location* *speciality* *page*"
            };
            var query = new JobsQuery { Speciality = "c# developer", Location = "minsk" };

            _transliteratorMock
                .Setup(x => x.Transliterate(It.IsAny<string?>(), It.IsAny<JobFinderSetting?>()))
                .Returns("minsk-trans");
            _pageObserverMock
                .Setup(x => x.InitializeCounter(It.IsAny<PageCounterQuery>()))
                .Returns(new PageCounter("test") { CurrentPage = 0, HasNextPage = true });

            var loader = CreateLoader();

            var result = await loader.GetJobsAsync(setting, query);
            var job = Assert.Single(result);

            Assert.Equal("test", job.Logo?.Source);
            Assert.Equal("bad! minsk-trans c%23+developer 1", job.Logo?.Url);
            Assert.StartsWith("test Error:", job.Title);

            _transliteratorMock.Verify(x => x.Transliterate("minsk", setting), Times.Once);
            _pageObserverMock.Verify(x => x.InitializeCounter(It.Is<PageCounterQuery>(
                q => q.Source == "test" && q.Speciality == "c# developer" && q.Location == "minsk")), Times.Once);
            _pageObserverMock.Verify(x => x.UpdateCounterAsync(It.IsAny<PageCounterQuery?>()), Times.Never);
        }

        [Theory]
        [InlineData(false, "bad! 3")]
        [InlineData(true, "bad! 2")]
        public async Task GetJobsAsync_AppliesPaginationModeToPageNumber(bool zeroBased, string expectedUrl)
        {
            var setting = new JobFinderSetting
            {
                Source = "test",
                LinkTemplate = "bad! *page*",
                ZeroBasedPagination = zeroBased
            };

            _pageObserverMock
                .Setup(x => x.InitializeCounter(It.IsAny<PageCounterQuery>()))
                .Returns(new PageCounter("test") { CurrentPage = 2, HasNextPage = true });

            var loader = CreateLoader();

            var result = await loader.GetJobsAsync(setting, new JobsQuery());
            var job = Assert.Single(result);

            Assert.Equal(expectedUrl, job.Logo?.Url);
        }
    }
}