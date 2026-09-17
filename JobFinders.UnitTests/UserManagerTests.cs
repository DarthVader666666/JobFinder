using System.Text.RegularExpressions;

using JobFinders.Application.Services;
using JobFinders.Domain.Entities;
using JobFinders.Domain.Interfaces;

using Moq;
using Xunit;

namespace JobFinders.UnitTests
{
    public class UserManagerTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IRepository<User>> _usersRepoMock;
        private readonly Mock<IRepository<ConfirmationCode>> _confirmationCodesRepoMock;
        private readonly Mock<IRepository<UserRole>> _userRolesRepoMock;

        public UserManagerTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _usersRepoMock = new Mock<IRepository<User>>();
            _confirmationCodesRepoMock = new Mock<IRepository<ConfirmationCode>>();
            _userRolesRepoMock = new Mock<IRepository<UserRole>>();

            _unitOfWorkMock.SetupGet(x => x.Users).Returns(_usersRepoMock.Object);
            _unitOfWorkMock.SetupGet(x => x.ConfirmationCodes).Returns(_confirmationCodesRepoMock.Object);
            _unitOfWorkMock.SetupGet(x => x.UserRoles).Returns(_userRolesRepoMock.Object);
        }

        private UserManager CreateManager()
        {
            return new UserManager(_unitOfWorkMock.Object);
        }

        [Fact]
        public void TryGetUserByEmail_WhenUserExists_ReturnsTrueAndSetsUser()
        {
            var existingUser = new User { UserId = 1, Email = "user@example.com" };
            _usersRepoMock
                .Setup(x => x.GetBy(It.IsAny<object>()))
                .Returns(existingUser);
            var manager = CreateManager();

            var result = manager.TryGetUserByEmail("user@example.com", out User? user);

            Assert.True(result);
            Assert.Same(existingUser, user);
        }

        [Fact]
        public void TryGetUserByEmail_WhenUserDoesNotExist_ReturnsFalseAndNullUser()
        {
            _usersRepoMock
                .Setup(x => x.GetBy(It.IsAny<object>()))
                .Returns((User?)null);
            var manager = CreateManager();

            var result = manager.TryGetUserByEmail("missing@example.com", out User? user);

            Assert.False(result);
            Assert.Null(user);
        }

        [Fact]
        public void TryGetUserByEmail_WhenEmailIsNull_QueriesWithEmptyString()
        {
            _usersRepoMock
                .Setup(x => x.GetBy(""))
                .Returns(new User { UserId = 1, Email = "user@example.com" });
            var manager = CreateManager();

            var result = manager.TryGetUserByEmail(null, out User? user);

            Assert.True(result);
            Assert.NotNull(user);
            _usersRepoMock.Verify(x => x.GetBy(string.Empty), Times.Once);
        }

        [Fact]
        public void GetCode_WhenUserIsNull_ThrowsArgumentNullException()
        {
            var manager = CreateManager();

            var exception = Assert.Throws<ArgumentNullException>(() => manager.GetCode(null));

            Assert.Equal("user", exception.ParamName);
        }

        [Fact]
        public void GetCode_WhenConfirmationCodeExists_ReturnsCode()
        {
            var user = new User { UserId = 7 };
            _confirmationCodesRepoMock
                .Setup(x => x.GetBy(It.IsAny<object>()))
                .Returns(new ConfirmationCode { UserId = 7, Code = "1234" });
            var manager = CreateManager();

            var code = manager.GetCode(user);

            Assert.Equal("1234", code);
            _confirmationCodesRepoMock.Verify(x => x.GetBy(user.UserId), Times.Once);
        }

        [Fact]
        public void GetCode_WhenConfirmationCodeNotFound_ReturnsNull()
        {
            var user = new User { UserId = 7 };
            _confirmationCodesRepoMock
                .Setup(x => x.GetBy(It.IsAny<object>()))
                .Returns((ConfirmationCode?)null);
            var manager = CreateManager();

            var code = manager.GetCode(user);

            Assert.Null(code);
        }

        [Fact]
        public async Task RegisterUser_WhenUserAlreadyExists_ReturnsFalseAndDoesNotCreateAnything()
        {
            _usersRepoMock
                .Setup(x => x.GetBy(It.IsAny<object>()))
                .Returns(new User { UserId = 1, Email = "user@example.com" });
            var manager = CreateManager();

            var result = await manager.RegisterUser("user@example.com", "password", "1234");

            Assert.False(result);
            _usersRepoMock.Verify(x => x.CreateAsync(It.IsAny<User>()), Times.Never);
            _confirmationCodesRepoMock.Verify(x => x.CreateAsync(It.IsAny<ConfirmationCode>()), Times.Never);
            _userRolesRepoMock.Verify(x => x.CreateAsync(It.IsAny<UserRole>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task RegisterUser_WhenUserIsNew_CreatesUserConfirmationCodeAndRole()
        {
            const string email = "new@example.com";
            const string password = "secret";
            const string code = "5678";

            _usersRepoMock
                .Setup(x => x.GetBy(It.IsAny<object>()))
                .Returns((User?)null);
            _usersRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Callback<User>(u => u.UserId = 42)
                .ReturnsAsync((User u) => u);
            var manager = CreateManager();

            var result = await manager.RegisterUser(email, password, code);

            Assert.True(result);
            _usersRepoMock.Verify(x => x.CreateAsync(It.Is<User>(u =>
                u.Email == email && u.Password == password)), Times.Once);
            _confirmationCodesRepoMock.Verify(x => x.CreateAsync(It.Is<ConfirmationCode>(c =>
                c.UserId == 42 && c.Code == code)), Times.Once);
            _userRolesRepoMock.Verify(x => x.CreateAsync(It.Is<UserRole>(r => r.UserId == 42)), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
        }

        [Fact]
        public async Task RegisterUser_SetsConfirmationCodeExpirationToThreeHoursAndOneMinuteFromUtcNow()
        {
            ConfirmationCode? captured = null;
            _usersRepoMock
                .Setup(x => x.GetBy(It.IsAny<object>()))
                .Returns((User?)null);
            _usersRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<User>()))
                .Callback<User>(u => u.UserId = 42)
                .ReturnsAsync((User u) => u);
            _confirmationCodesRepoMock
                .Setup(x => x.CreateAsync(It.IsAny<ConfirmationCode>()))
                .Callback<ConfirmationCode>(c => captured = c)
                .ReturnsAsync((ConfirmationCode c) => c);
            var manager = CreateManager();

            await manager.RegisterUser("new@example.com", "password", "1234");

            Assert.NotNull(captured);
            Assert.InRange(
                captured!.ExpirationTime,
                DateTime.UtcNow.AddHours(3).AddMinutes(1).AddSeconds(-30),
                DateTime.UtcNow.AddHours(3).AddMinutes(1).AddSeconds(30));
        }

        [Fact]
        public async Task ConfirmUser_MarksUserConfirmedAndSaves()
        {
            var user = new User { UserId = 1, Confirmed = false };
            _usersRepoMock
                .Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);
            var manager = CreateManager();

            await manager.ConfirmUser(user);

            Assert.True(user.Confirmed);
            _usersRepoMock.Verify(x => x.UpdateAsync(user), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task GenerateCodeAsync_WhenUserIsNull_ThrowsArgumentNullException()
        {
            var manager = CreateManager();

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => manager.GenerateCodeAsync(null));

            Assert.Equal("user", exception.ParamName);
        }

        [Fact]
        public async Task GenerateCodeAsync_WhenConfirmationCodeNotFound_ThrowsNullReferenceException()
        {
            _confirmationCodesRepoMock
                .Setup(x => x.GetBy(It.IsAny<object>()))
                .Returns((ConfirmationCode?)null);
            var manager = CreateManager();

            await Assert.ThrowsAsync<NullReferenceException>(() => manager.GenerateCodeAsync(new User { UserId = 1 }));

            _confirmationCodesRepoMock.Verify(x => x.UpdateAsync(It.IsAny<ConfirmationCode>()), Times.Never);
        }

        [Fact]
        public async Task GenerateCodeAsync_WhenConfirmationCodeExists_ReturnsFourDigitCodeAndRefreshesCode()
        {
            var user = new User { UserId = 1 };
            var existing = new ConfirmationCode { UserId = 1, Code = "old", ExpirationTime = DateTime.UtcNow.AddHours(1) };

            ConfirmationCode? updated = null;
            _confirmationCodesRepoMock
                .Setup(x => x.GetBy(It.IsAny<object>()))
                .Returns(existing);
            _confirmationCodesRepoMock
                .Setup(x => x.UpdateAsync(It.IsAny<ConfirmationCode>()))
                .Callback<ConfirmationCode>(c => updated = c)
                .ReturnsAsync((ConfirmationCode c) => c);
            var manager = CreateManager();

            var code = await manager.GenerateCodeAsync(user);

            Assert.Matches(@"^\d{4}$", code);
            Assert.NotNull(updated);
            Assert.Same(existing, updated);
            Assert.Equal(code, updated!.Code);
            Assert.InRange(
                updated.ExpirationTime,
                DateTime.UtcNow.AddHours(3).AddMinutes(1).AddSeconds(-30),
                DateTime.UtcNow.AddHours(3).AddMinutes(1).AddSeconds(30));
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public void GenerateCode_ReturnsFourDigitCode()
        {
            var manager = CreateManager();

            for (int i = 0; i < 100; i++)
            {
                var code = manager.GenerateCode();

                Assert.Matches(@"^\d{4}$", code);
            }
        }

        [Fact]
        public void IsCodeExpired_WhenUserIsNull_ThrowsArgumentNullException()
        {
            var manager = CreateManager();

            var exception = Assert.Throws<ArgumentNullException>(() => manager.IsCodeExpired(null, out _));

            Assert.Equal("user", exception.ParamName);
        }

        [Fact]
        public void IsCodeExpired_WhenNoConfirmationCodeFound_ThrowsNullReferenceException()
        {
            _confirmationCodesRepoMock
                .Setup(x => x.GetAll())
                .Returns(Array.Empty<ConfirmationCode>().AsQueryable());
            var manager = CreateManager();

            Assert.Throws<NullReferenceException>(() => manager.IsCodeExpired(new User { UserId = 1 }, out _));
        }

        [Fact]
        public void IsCodeExpired_WhenCodeExpired_ReturnsTrueAndSetsCode()
        {
            var user = new User { UserId = 1 };
            var codeEntity = new ConfirmationCode { UserId = 1, Code = "1234", ExpirationTime = DateTime.UtcNow.AddMinutes(-1) };
            _confirmationCodesRepoMock
                .Setup(x => x.GetAll())
                .Returns(new[] { codeEntity }.AsQueryable());
            var manager = CreateManager();

            var result = manager.IsCodeExpired(user, out ConfirmationCode? code);

            Assert.True(result);
            Assert.Same(codeEntity, code);
        }

        [Fact]
        public void IsCodeExpired_WhenCodeNotExpired_ReturnsFalse()
        {
            var user = new User { UserId = 1 };
            var codeEntity = new ConfirmationCode { UserId = 1, Code = "1234", ExpirationTime = DateTime.UtcNow.AddHours(5) };
            _confirmationCodesRepoMock
                .Setup(x => x.GetAll())
                .Returns(new[] { codeEntity }.AsQueryable());
            var manager = CreateManager();

            var result = manager.IsCodeExpired(user, out ConfirmationCode? code);

            Assert.False(result);
            Assert.Same(codeEntity, code);
        }
    }
}