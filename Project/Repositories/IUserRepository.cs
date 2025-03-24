using Project.Models;

namespace Project.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByEmail(string email);
        Task<List<User>> GetAllUser();
        Task<User> GetUserByUserID(int id);
        Task<bool> CreateUser(User user);
        Task<bool> UpdateUser(User user);
        Task<bool> DeleteUser(int id);
    }
}
