using Project.Models;

namespace Project.Service
{
    public interface IUserSerivce
    {
        Task<User> ValidateUser(string email, string password);
        Task<bool> IsEmailExist(string email);
        Task<List<User>> GetAllUser();
    }
}
