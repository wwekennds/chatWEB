using Chat.Models;
using System.Collections.Concurrent;

namespace Chat.Services
{
    public class UserService : IUserService
    {
        private static readonly ConcurrentDictionary<string, List<string>> _usersInRoom = new ConcurrentDictionary<string, List<string>>();

        public async Task<UserList> GetUsersFromRoom(string roomName)
        {
            if (_usersInRoom.TryGetValue(roomName, out var users))
            {
                return new UserList { UserInRoom = users };
            }
            
            return new UserList { UserInRoom = new List<string>() };
        }

        public void AddUserToRoom(string roomName, string userName)
        {
            _usersInRoom.AddOrUpdate(
                roomName,
                new List<string> { userName },
                (key, existingUsers) =>
                {
                    if (!existingUsers.Contains(userName))
                    {
                        existingUsers.Add(userName);
                    }
                    return existingUsers;
                }
            );
        }

        public void RemoveUserFromRoom(string roomName, string userName)
        {
            if (_usersInRoom.TryGetValue(roomName, out var users))
            {
                users.Remove(userName);
                
                if (users.Count == 0)
                {
                    _usersInRoom.TryRemove(roomName, out _);
                }
            }
        }
    }
}