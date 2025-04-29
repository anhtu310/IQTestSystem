using Microsoft.EntityFrameworkCore;
using Project.Models;

namespace Project.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IqtestSystemContext context;
        public UserRepository(IqtestSystemContext context)
        {
            this.context = context;
        }

        Task<bool> IUserRepository.CreateUser(User user)
        {
            throw new NotImplementedException();
        }

        Task<bool> IUserRepository.DeleteUser(int id)
        {
            throw new NotImplementedException();
        }

        Task<List<User>> IUserRepository.GetAllUser()
        {
            throw new NotImplementedException();
        }

        async Task<User> IUserRepository.GetUserByEmail(string email)
        {
            return await context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        Task<User> IUserRepository.GetUserByUserID(int id)
        {
            throw new NotImplementedException();
        }

        Task<bool> IUserRepository.UpdateUser(User user)
        {
            throw new NotImplementedException();
        }
    }
}
