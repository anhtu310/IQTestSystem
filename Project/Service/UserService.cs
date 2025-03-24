using System.Security.Cryptography;
using System.Text;
using Project.Models;
using Project.Repositories;

namespace Project.Service
{
    public class UserService : IUserSerivce
    {
        private readonly IUserRepository userRepository;
        public UserService(IUserRepository userRepository )
        {
           this.userRepository = userRepository;
        }

        Task<List<User>> IUserSerivce.GetAllUser()
        {
            throw new NotImplementedException();
        }

        Task<bool> IUserSerivce.IsEmailExist(string email)
        {
            throw new NotImplementedException();
        }

        async Task<User> IUserSerivce.ValidateUser(string email, string password)
        {
            var user = await userRepository.GetUserByEmail(email);
            if(user != null && user.PasswordHash == HashPassword(password))
            {
                return user;
            }
            return null;
        }

        private string HashPassword(string password)
        {
            // Mã hóa password bằng SHA256
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(
                    Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
