using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using Chat.Models;
using System.Collections.Concurrent;
using Chat.Services;

namespace Chat.Hubs;

public interface IChatClient
{
    public Task ReceiveMessage(string userName, string message);
}

public class ChatHub : Hub<IChatClient>
{
    private readonly IDistributedCache _cache;
    private readonly IUserService _userService;

    public ChatHub(IDistributedCache cache, IUserService userService)
    {
        _cache = cache;
        _userService = userService;
    }

    public async Task JoinChat(UserConnection connection)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, connection.ChatRoom);

          _userService.AddUserToRoom(connection.ChatRoom, connection.UserName);
        
        var stringConnection = JsonSerializer.Serialize(connection);

        await _cache.SetStringAsync(Context.ConnectionId, stringConnection);

        await Clients
            .Group(connection.ChatRoom)
            .ReceiveMessage("Admin", $"{connection.UserName} присоединился к чату");
    }

    public async Task SendMessage(string message)
    {
        var stringConnection = await _cache.GetAsync(Context.ConnectionId);

        var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection);

        if (connection is not null)
        {
            await Clients
                .Group(connection.ChatRoom)
                .ReceiveMessage(connection.UserName, message);
        }
    }


    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var stringConnection = await _cache.GetAsync(Context.ConnectionId);
        var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection);

        if (connection is not null)
        {
            await _cache.RemoveAsync(Context.ConnectionId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, connection.ChatRoom);

             _userService.RemoveUserFromRoom(connection.ChatRoom, connection.UserName);


            await Clients
                    .Group(connection.ChatRoom)
                    .ReceiveMessage("Admin", $"{connection.UserName} покинул чат");
        }

        await base.OnDisconnectedAsync(exception);
    }
}