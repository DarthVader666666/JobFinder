using JobFinders.Domain.Entities;
using JobFinders.Domain.Interfaces;

using Microsoft.EntityFrameworkCore;

namespace JobFinders.Application.Services
{
    public class UserManager(IUnitOfWork unitOfWork) : IUserManager
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public User? GetUserByEmail(string? email)
        {
            throw new NotImplementedException();
        }

        public User? GetUserById(int id)
        {
            throw new NotImplementedException();
        }

        public User? GetUserByCode(string code)
        {
            var user = _unitOfWork.ConfirmationCodes.GetAll().Include(c => c.User).FirstOrDefault(c => c.Code == code)?.User;
            return user;
        }

        public Task<bool> LogIn(User? user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Register(User user)
        {
            throw new NotImplementedException();
        }

        public bool DoesUserExist(string? email)
        {
            var result = _unitOfWork.Users.GetAll().FirstOrDefault(u => u.Email == email) is not null;
            return result;
        }

        public async Task<string> GenerateCodeAsync(string? email)
        {
            var random = new Random();
            var code = random.Next(0, 10000).ToString("D4");

            User? user;
            ConfirmationCode? confirmationCode;

            if (DoesUserExist(email))
            {
                user = _unitOfWork.Users.Get(email ?? string.Empty) ?? throw new NullReferenceException(nameof(user));
                confirmationCode = _unitOfWork.ConfirmationCodes.Get(user.UserId) ?? throw new NullReferenceException(nameof(confirmationCode));

                confirmationCode.DateGenerated = DateTime.UtcNow.AddHours(3);
                confirmationCode.Code = code;

                await _unitOfWork.ConfirmationCodes.UpdateAsync(confirmationCode);
            }
            else
            {
                user = new User { Email = email };
                await _unitOfWork.Users.CreateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var newUser = GetUserByEmail(email) ?? throw new NullReferenceException(nameof(user));
                confirmationCode = new ConfirmationCode { UserId = newUser!.UserId, DateGenerated = DateTime.UtcNow.AddHours(3), Code = code };
                await _unitOfWork.ConfirmationCodes.CreateAsync(confirmationCode);
            }
            
            await _unitOfWork.SaveChangesAsync();

            return code;
        }

        public bool CodeExpired(string code)
        {
            var confirmationCode = _unitOfWork.ConfirmationCodes.GetAll().FirstOrDefault(x => x.Code == code) ?? throw new NullReferenceException("Не найден ConfirmationCode");

            var now = DateTime.UtcNow.AddHours(3);
            var span = now - confirmationCode.DateGenerated;

            return span < TimeSpan.FromSeconds(60);
        }
    }
}
