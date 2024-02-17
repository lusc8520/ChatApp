using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using ChatRoom = de.hsfl.vs.hul.chatApp.contract.DTO.ChatRoom;
using User = de.hsfl.vs.hul.chatApp.server.DAO.User;
using UserDTO = de.hsfl.vs.hul.chatApp.contract.DTO.User;

namespace de.hsfl.vs.hul.chatApp.server;

[ServiceBehavior(
    InstanceContextMode = InstanceContextMode.PerSession,
    IncludeExceptionDetailInFaults = true)]
public class ChatService : IChatService
{
    private static readonly string DbPath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName + "/data.db";
    private static readonly ConcurrentDictionary<int, IChatClient> Clients = new(); // for global chat ?
    
    public void Connect()
    {
        Console.WriteLine("client connected!");
        var client = OperationContext.Current.GetCallbackChannel<IChatClient>();
        client?.Connect();
    }

    private async Task ConnectToGlobalChat(int userId, IChatClient client)
    {
        await Task.Run(() =>
        {
            Clients[userId] = client;
            // TODO send broadcast for global chat ?
        });
    }

    public LoginResponse Login(string username, string password)
    {
        var db = new SQLiteConnection($"Data Source={DbPath}");
        db.Open();
        // get user with the given username from the db
        var user = db.QuerySingleOrDefault<User?>(
            $"select * from Users where username = @Username", 
            new {Username = username}
        );
        db.Close();
        if (user != null && user.Password == password)
        {
            // login success
            ConnectToGlobalChat(user.Id, OperationContext.Current.GetCallbackChannel<IChatClient>());
            return new LoginResponse { User = user.ToDto()};
        }
        // login failed
        return new LoginResponse
        {
            Text = "wrong username or password"
        };
    }

    public LoginResponse Register(string username, string password)
    {
        if (!ValidateInput(username, password))
        {
            return new LoginResponse { Text = "username and password need a minimum length of 3" };
        }
        var db = new SQLiteConnection($"Data Source={DbPath}");
        db.Open();
        // check if the username already exists
        var existUser = db.QuerySingleOrDefault<User?>(
            $"select * from Users where username = @Username", new {Username = username}
        );
        db.Close();
        if (existUser != null)
        {
            // username already exists
            return new LoginResponse { Text = "username is not available" };
        }
        
        // username does not exist -> do register
        // TODO encrypt password ?
        var user = new User
        {
            Username = username,
            Password = password
        };
        db.Open();
        int userId = (int)db.Insert(user);
        db.Close();
        ConnectToGlobalChat(userId, OperationContext.Current.GetCallbackChannel<IChatClient>());
        return new LoginResponse {User = user.ToDto()};
    }

    public List<ChatRoom> FetchChatRooms()
    {
        var db = new SQLiteConnection($"Data Source={DbPath}");
        db.Open();
        // get all chat rooms and map them to DTOs
        var rooms = db.Query<ChatRoom>(
            @"select * from Chatrooms").AsList();
        db.Close();
        return rooms;
    }

    public List<UserDTO> FetchUsers()
    {
        var db = new SQLiteConnection($"Data Source={DbPath}");
        db.Open();
        // get all users by and map them to DTOs
        var users = db.Query<User>(
            @"select * from Users"
        ).Select(user => user.ToDto()).AsList();
        return users;
    }

    public void SendMessage(Message message)
    {
        message.DateTime = DateTime.Now;
        foreach (var client in Clients)
        {
            try
            {
                client.Value.ReceiveMessage(message);
            }
            catch (Exception e)
            {
                Clients.TryRemove(client.Key, out _);
                Console.WriteLine(e);
            }
        }
    }

    private bool ValidateInput(string? username, string? password)
    {
        // TODO make input validation safer?
        if (username == null || password == null) return false;
        return !(username.Length < 3 || password.Length < 3);
    }
}