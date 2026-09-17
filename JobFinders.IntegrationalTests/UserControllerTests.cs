using System.Net;
using System.Net.Http.Json;

using JobFinders.Domain.Interfaces;

using Moq;
using Xunit;

namespace JobFinders.IntegrationalTests
{
    public class UserControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public UserControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SendComment_WhenRequestIsValid_ReturnsOkAndSendsEmail()
        {
            _factory.EmailSenderMock
                .Setup(x => x.SendEmailAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(true);

            var response = await _client.PostAsJsonAsync("/User/SendComment", new { comment = "Great service!" });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            _factory.EmailSenderMock.Verify(
                x => x.SendEmailAsync(
                    "recipient@example.com",
                    "Отзыв от пользователя JobFinders",
                    It.Is<string>(body => body.Contains("Great service!"))),
                Times.Once);
        }

        [Fact]
        public async Task SendComment_WhenBodyIsNull_ReturnsBadRequest()
        {
            var response = await _client.PostAsJsonAsync("/User/SendComment", (object?)null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task SendComment_WhenEmailSendFails_ReturnsServerError()
        {
            _factory.EmailSenderMock
                .Setup(x => x.SendEmailAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(false);

            var response = await _client.PostAsJsonAsync("/User/SendComment", new { comment = "Hello" });

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }
    }
}