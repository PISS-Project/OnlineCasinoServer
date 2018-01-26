using OnlineCasinoServer.Core.DTOs;

namespace OnlineCasinoServer.Core.Repositories
{
    public interface IUserRepository
    {
        UserDto Get(int id);
        UserDto Get(string username, string password);
        UserDto Create(UserDto user);
        bool IsPasswordCorrect(int id, string password);
        UserDto UpdateNameAndEmail(UserDto user);
        void UpdatePassword(int id, string oldPassword, string newPassword);
        void Delete(int id);
        UserDto AddMoney(int id, decimal money);
        void UpdateMoney(int id, decimal newMoney);
    }
}
