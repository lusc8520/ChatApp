using System;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using User = de.hsfl.vs.hul.chatApp.server.DAO.User;

namespace de.hsfl.vs.hul.chatApp.server;

[ServiceBehavior(
    InstanceContextMode = InstanceContextMode.PerSession,
    UseSynchronizationContext = false,
    IncludeExceptionDetailInFaults = true)]
public class ChatService : IChatService
{
    private static readonly string DbPath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName + "/data.db";
    private static readonly ConcurrentDictionary<User, IChatClient> Clients = new(); // for global chat ?
    
    public void Connect()
    {
        Console.WriteLine("client connected!");
        var client = OperationContext.Current.GetCallbackChannel<IChatClient>();
        client?.Connect();
    }

    private async Task ConnectToGlobalChat(User user, IChatClient? client)
    {
        await Task.Run(() =>
        {
            if (client == null) return;
            Clients[user] = client;
            // TODO send broadcast for global chat ?
        });
    }

    public LoginResponse Login(string username, string password)
    {
        var db = new SQLiteConnection($"Data Source={DbPath}");
        db.Open();
        var user = db.QueryFirstOrDefault<User?>($"select * from user where username = @Username", new {Username = username});
        db.Close();
        if (user != null && user.Password == password)
        {
            // login success
            ConnectToGlobalChat(user, OperationContext.Current.GetCallbackChannel<IChatClient>());
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
        var existUser = db.QuerySingleOrDefault<User?>(
            $"select * from user where username = @Username", new {Username = username}
        );
        db.Close();
        if (existUser != null)
        {
            // username already exists
            return new LoginResponse { Text = "username is not available" };
        }
        
        // do register
        // TODO encrypt password ?
        var user = new User
        {
            Username = username,
            Password = password
        };
        db.Open();
        db.Insert(user);
        db.Close();
        ConnectToGlobalChat(user, OperationContext.Current.GetCallbackChannel<IChatClient>()?? null);
        return new LoginResponse {User = user.ToDto()};
    }

    private bool ValidateInput(string? username, string? password)
    {
        if (username == null || password == null) return false;
        return !(username.Length < 3 || password.Length < 3);
    }
}