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
    public Task ReceiveFile(string userName, string fileName, string fileUrl, long fileSize); 
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

    public async Task SendFileMessage(string fileName, string fileUrl, long fileSize)
    {
        var stringConnection = await _cache.GetAsync(Context.ConnectionId);
        var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection);

        if (connection is not null)
        {
            await Clients
                .Group(connection.ChatRoom)
                .ReceiveFile(connection.UserName, fileName, fileUrl, fileSize);
        }
    }

    // public async Task SendFileMessage(string fileName, string fileUrl, long fileSize)
    // {
    //     try
    //     {
    //         Console.WriteLine($"SendFileMessage вызван: {fileName}, {fileUrl}, {fileSize}");

    //         var stringConnection = await _cache.GetAsync(Context.ConnectionId);
    //         if (stringConnection == null)
    //         {
    //             Console.WriteLine("Connection not found in cache");
    //             return;
    //         }

    //         var connection = JsonSerializer.Deserialize<UserConnection>(stringConnection);
    //         if (connection is not null)
    //         {
    //             Console.WriteLine($"Отправка файла в комнату: {connection.ChatRoom}, пользователь: {connection.UserName}");

    //             await Clients
    //                 .Group(connection.ChatRoom)
    //                 .ReceiveFile(connection.UserName, fileName, fileUrl, fileSize);

    //             Console.WriteLine("Сообщение о файле отправлено всем клиентам");
    //         }
    //         else
    //         {
    //             Console.WriteLine("Connection is null");
    //         }
    //     }
    //     catch (Exception ex)
    //     {
    //         Console.WriteLine($"Ошибка в SendFileMessage: {ex.Message}");
    //         throw;
    //     }
    // }

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