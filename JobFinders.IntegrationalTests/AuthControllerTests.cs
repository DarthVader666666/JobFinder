using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

using JobFinders.DAL;
using JobFinders.Domain.Entities;

using Microsoft.Extensions.DependencyInjection;

using Moq;
using Xunit;

namespace JobFinders.IntegrationalTests
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;

        public AuthControllerTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task SignUp_WithNewEmailSendsConfirmationCodeAndRegistersUser()
        {
            const string email = "authSignUp@example.com";
            var sentCode = await SignUpAsync(email, "password1");

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<JobFinderDbContext>();

            var user = db.Users.FirstOrDefault(u => u.Email == email);
            Assert.NotNull(user);
            var confirmationCode = db.ConfirmationCodes.FirstOrDefault(c => c.UserId == user!.UserId);
            Assert.NotNull(confirmationCode);
            Assert.Equal(sentCode, confirmationCode!.Code);
            Assert.NotNull(db.UserRoles.FirstOrDefault(ur => ur.UserId == user!.UserId));

            _factory.EmailSenderMock.Verify(
                x => x.SendEmailAsync(email, "Код подтверждения", sentCode),
                Times.Once);
        }

        [Fact]
        public async Task SignUp_WhenUserAlreadyExists_ReturnsBadRequestWithoutSendingNewCode()
        {
            const string email = "authExisting@example.com";
            await SignUpAsync(email, "password1");

            var response = await PostAsync("/Auth/SignUp", ("Email", email), ("Password", "password1"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorText = await GetErrorTextAsync(response);
            Assert.Contains("уже зарегестрирован", errorText);

            _factory.EmailSenderMock.Verify(
                x => x.SendEmailAsync(email, It.IsAny<string?>(), It.IsAny<string?>()),
                Times.Once);
        }

        [Fact]
        public async Task SignUp_WhenEmailSendFails_ReturnsBadRequestAndDoesNotRegisterUser()
        {
            const string email = "authSendFails@example.com";
            _factory.EmailSenderMock
                .Setup(x => x.SendEmailAsync(email, It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(false);

            var response = await PostAsync("/Auth/SignUp", ("Email", email), ("Password", "password1"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Не удалось отправить код подтверждения", await GetErrorTextAsync(response));

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<JobFinderDbContext>();
            Assert.Null(db.Users.FirstOrDefault(u => u.Email == email));
        }

        [Fact]
        public async Task SignUp_WhenEmailSenderThrowsFormatException_ReturnsBadRequest()
        {
            const string email = "authBadFormat@example.com";
            _factory.EmailSenderMock
                .Setup(x => x.SendEmailAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .ThrowsAsync(new Exception("Invalid email format"));

            var response = await PostAsync("/Auth/SignUp", ("Email", email), ("Password", "password1"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Неверный формат адреса почты", await GetErrorTextAsync(response));
        }

        [Fact]
        public async Task SignUp_WhenEmailHeaderMissing_ReturnsServerError()
        {
            var response = await PostAsync("/Auth/SignUp", ("Password", "password1"));

            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        [Fact]
        public async Task SignInWithPassword_WhenCredentialsAreValid_ReturnsOkSetsCookieAndConfirmsUser()
        {
            const string email = "authPwValid@example.com";
            const string password = "password1";
            await SignUpAsync(email, password);

            var response = await PostAsync("/Auth/SignInWithPassword", ("Email", email), ("Password", password));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("access_token=", response.Headers.GetValues("Set-Cookie").First());

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<JobFinderDbContext>();
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            Assert.NotNull(user);
            Assert.True(user!.Confirmed);
        }

        [Fact]
        public async Task SignInWithPassword_WhenPasswordIsWrong_ReturnsUnauthorized()
        {
            const string email = "authPwWrong@example.com";
            await SignUpAsync(email, "password1");

            var response = await PostAsync("/Auth/SignInWithPassword", ("Email", email), ("Password", "wrong"));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("Неверный пароль", await GetErrorTextAsync(response));
        }

        [Fact]
        public async Task SignInWithPassword_WhenUserNotFound_ReturnsBadRequest()
        {
            var response = await PostAsync("/Auth/SignInWithPassword", ("Email", "missing@example.com"), ("Password", "x"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Пользователь не найден", await GetErrorTextAsync(response));
        }

        [Fact]
        public async Task SignInWithCode_WhenCodeIsValid_ReturnsOkSetsCookieAndConfirmsUser()
        {
            const string email = "authCodeValid@example.com";
            var code = await SignUpAsync(email, "password1");

            var response = await PostAsync("/Auth/SignInWithCode", ("Email", email), ("Code", code));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("access_token=", response.Headers.GetValues("Set-Cookie").First());

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<JobFinderDbContext>();
            Assert.True(db.Users.FirstOrDefault(u => u.Email == email)!.Confirmed);
        }

        [Fact]
        public async Task SignInWithCode_WhenCodeIsWrong_ReturnsUnauthorized()
        {
            const string email = "authCodeWrong@example.com";
            await SignUpAsync(email, "password1");

            var response = await PostAsync("/Auth/SignInWithCode", ("Email", email), ("Code", "9999"));

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal("Неверный код", await GetErrorTextAsync(response));
        }

        [Fact]
        public async Task SignInWithCode_WhenCodeIsExpired_ReturnsBadRequest()
        {
            const string email = "authCodeExpired@example.com";
            var sentCode = await SignUpAsync(email, "password1");

            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<JobFinderDbContext>();
                var user = db.Users.First(u => u.Email == email);
                var code = db.ConfirmationCodes.First(c => c.UserId == user.UserId);
                code.ExpirationTime = DateTime.UtcNow.AddHours(2).AddMinutes(-1);
                db.SaveChanges();
            }

            var response = await PostAsync("/Auth/SignInWithCode", ("Email", email), ("Code", sentCode));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Код подтверждения истёк", await GetErrorTextAsync(response));
        }

        [Fact]
        public async Task SendCode_WhenEmailIsNull_ReturnsBadRequest()
        {
            var response = await PostAsync("/Auth/SendCode");

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Email is null", await GetErrorTextAsync(response));
        }

        [Fact]
        public async Task SendCode_WhenUserNotFound_ReturnsBadRequest()
        {
            var response = await PostAsync("/Auth/SendCode", ("Email", "missing@example.com"));

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("Пользователь не найден", await GetErrorTextAsync(response));
        }

        [Fact]
        public async Task SendCode_WhenUserExists_ReturnsOkAndSendsNewCode()
        {
            const string email = "authSendCode@example.com";
            await SignUpAsync(email, "password1");

            string? resentCode = null;
            _factory.EmailSenderMock
                .Setup(x => x.SendEmailAsync(email, It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(true)
                .Callback((string? to, string? subject, string? body) => resentCode = body);

            var response = await PostAsync("/Auth/SendCode", ("Email", email));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(resentCode);
            Assert.Matches(@"^\d{4}$", resentCode);

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<JobFinderDbContext>();
            var user = db.Users.First(u => u.Email == email);
            var code = db.ConfirmationCodes.First(c => c.UserId == user.UserId);
            Assert.Equal(resentCode, code.Code);

            _factory.EmailSenderMock.Verify(
                x => x.SendEmailAsync(email, "Код подтверждения", resentCode),
                Times.AtLeastOnce);
        }

        private async Task<string> SignUpAsync(string email, string password)
        {
            string? sentCode = null;

            _factory.EmailSenderMock
                .Setup(x => x.SendEmailAsync(email, It.IsAny<string?>(), It.IsAny<string?>()))
                .ReturnsAsync(true)
                .Callback((string? to, string? subject, string? body) => sentCode = body);

            var response = await PostAsync("/Auth/SignUp", ("Email", email), ("Password", password));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(sentCode);

            return sentCode!;
        }

        private async Task<HttpResponseMessage> PostAsync(string url, params (string Name, string Value)[] headers)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);

            foreach (var (name, value) in headers)
            {
                request.Headers.Add(name, value);
            }

            return await _client.SendAsync(request);
        }

        private static async Task<string?> GetErrorTextAsync(HttpResponseMessage response)
        {
            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return doc.RootElement.TryGetProperty("errorText", out var element) ? element.GetString() : null;
        }
    }
}