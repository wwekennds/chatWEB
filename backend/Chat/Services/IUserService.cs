using Chat.Models;
using Chat.Hubs;

namespace Chat.Services
{
   public interface IUserService
    {
        Task<UserList> GetUsersFromRoom(string roomName);
        void AddUserToRoom(string roomName, string userName);
        void RemoveUserFromRoom(string roomName, string userName);
    }
}