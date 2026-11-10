using JobFinders.Domain.Entities;
using JobFinders.Domain.Interfaces;

namespace JobFinders.Application.Services
{
    public class UserManager(IUnitOfWork unitOfWork, ICryptoService cryptoService) : IUserManager
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly ICryptoService _cryptoService = cryptoService;

        public bool TryGetUserByEmail(string? email, out User? user)
        {
            var encryptedEmail = _cryptoService.Encrypt(email);
            user = _unitOfWork.Users.GetBy(encryptedEmail ?? string.Empty);
            return user is not null;
        }

        public string[]? GetRoleNames(User? user)
        {
            if (user is null || user?.UserRoles?.Count == 0)
            {
                return null;
            }

            var roleIds = user?.UserRoles?.Select(ur => ur.RoleId) ?? [];
            var roleNames = _unitOfWork.Roles.GetAll().Where(role => roleIds.Contains(role.RoleId)).Select(ur => ur.RoleName);

            return roleNames.ToArray() as string[];
        }

        public string? GetCode(User? user)
        {
            ArgumentNullException.ThrowIfNull(user);
            return _unitOfWork.ConfirmationCodes.GetBy(user.UserId)?.Code;
        }

        public async Task<bool> RegisterUser(string? email, string? password, string? code)
        {
            if (TryGetUserByEmail(email, out User? user))
            {
                return false;
            }
            else
            {
                var encryptedEmail = _cryptoService.Encrypt(email);
                var encryptedPassword = _cryptoService.Encrypt(password);

                user = new User { Email = encryptedEmail, Password = encryptedPassword };
                await _unitOfWork.Users.CreateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var confirmationCode = new ConfirmationCode { UserId = user.UserId, Code = code, ExpirationTime = DateTime.UtcNow.AddHours(3).AddMinutes(1) };
                await _unitOfWork.ConfirmationCodes.CreateAsync(confirmationCode);

                var userRole = new UserRole { UserId = user.UserId };
                await _unitOfWork.UserRoles.CreateAsync(userRole);
                await _unitOfWork.SaveChangesAsync();

                return true;
            }
        }

        public async Task ConfirmUser(User? user)
        {
            user?.Confirmed = true;
            await _unitOfWork.Users.UpdateAsync(user!);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<string> GenerateOneTimeCodeAsync(User? user)
        {
            ArgumentNullException.ThrowIfNull(user);

            var code = GenerateOneTimeCode();
            var confirmationCode = _unitOfWork.ConfirmationCodes.GetBy(user.UserId) ?? throw new NullReferenceException("Код подтверждения не найден");
            confirmationCode.ExpirationTime = DateTime.UtcNow.AddHours(3).AddMinutes(1);
            confirmationCode.Code = code;
            await _unitOfWork.ConfirmationCodes.UpdateAsync(confirmationCode);
            await _unitOfWork.SaveChangesAsync();

            return code;
        }

        public string GenerateOneTimeCode()
        {
            var random = new Random();
            var code = random.Next(0, 10000).ToString("D4");

            return code;
        }

        public bool IsCodeExpired(User? user, out ConfirmationCode? code)
        {
            ArgumentNullException.ThrowIfNull(user);

            code = _unitOfWork.ConfirmationCodes.GetAll().FirstOrDefault(x => x.UserId == user.UserId) ?? throw new NullReferenceException("Не найден ConfirmationCode");
            var now = DateTime.UtcNow.AddHours(3);
            var expirationTime = code.ExpirationTime;

            return now >= expirationTime;
        }
    }
}
