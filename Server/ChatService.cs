using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Threading.Tasks;
using Dapper;
using Dapper.Contrib.Extensions;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using de.hsfl.vs.hul.chatApp.server.DAO;

namespace de.hsfl.vs.hul.chatApp.server;

[ServiceBehavior(
    InstanceContextMode = InstanceContextMode.PerCall,
    IncludeExceptionDetailInFaults = true
    )]
public class ChatService : IChatService
{
    private static readonly string DbPath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName + "/data.db";
    private static readonly string UploadPath = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.FullName + "/Upload";
    private static readonly ConcurrentDictionary<int, IChatClient> Clients = new();
    
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
        });
    }

    public LoginResponse Login(string username, string password)
    {
        var db = new SQLiteConnection($"Data Source={DbPath}");
        db.Open();
        var user = db.QuerySingleOrDefault<UserDao?>(
            $"select * from Users where username = @Username", 
            new {Username = username}
        );
        db.Close();
        // check if user exists
        if (user != null && user.Password == password)
        {
            // user exists -> check if already logged in
            if (Clients.TryGetValue(user.Id, out IChatClient client))
            {
                try
                {
                    client.Connect();
                    Console.WriteLine("huhh");
                    // user is already logged in
                    return new LoginResponse { Text = $"{user.Username} is already logged in" };
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            // login success
            ConnectToGlobalChat(user.Id, OperationContext.Current.GetCallbackChannel<IChatClient>());
            return new LoginResponse { UserDto = user.ToDto()};
        }
        // user does not exist
        return new LoginResponse
        {
            Text = "wrong username or password"
        };
    }

    public void Logout(int userId)
    {
        Clients.TryRemove(userId, out _);
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
        var existUser = db.QuerySingleOrDefault<UserDao?>(
            $"select * from Users where username = @Username", new {Username = username}
        );
        db.Close();
        if (existUser != null)
        {
            // username already exists
            return new LoginResponse { Text = "username is not available" };
        }
        
        // username does not exist -> do register
        var user = new UserDao
        {
            Username = username,
            Password = password
        };
        db.Open();
        var userId = (int)db.Insert(user);
        db.Close();
        user.Id = userId;
        ConnectToGlobalChat(userId, OperationContext.Current.GetCallbackChannel<IChatClient>());
        Broadcast(client =>
        {
            if (client.Key == userId) return;
            client.Value.ReceiveNewUser(user.ToDto());
        });
        return new LoginResponse {UserDto = user.ToDto()};
        // TODO: broadcast the new user for private chat
    }

    public IEnumerable<ChatRoom> FetchChatRooms()
    {
        var db = new SQLiteConnection($"Data Source={DbPath}");
        db.Open();
        // get all chat rooms and map them to DTOs
        var rooms = db.Query<ChatRoom>(
            @"select * from Chatrooms");
        db.Close();
        return rooms;
    }

    public IEnumerable<UserDto> FetchUsers()
    {
        var db = new SQLiteConnection($"Data Source={DbPath}");
        db.Open();
        // get all users by and map them to DTOs
        var users = db.Query<UserDao>(
            @"select * from Users"
        ).Select(user => user.ToDto());
        return users;
    }

    public void BroadcastMessage(TextMessage textMessage)
    {
        textMessage.DateTime = DateTime.Now;
        SaveMessage(textMessage); // safe message asynchronously
        Broadcast(client => client.Value.ReceiveBroadcast(textMessage)); // broadcast message asynchronously
    }

    private async Task Broadcast(Action<KeyValuePair<int, IChatClient>> a)
    {
        await Task.Run(() =>
        {
            foreach (var client in Clients)
            {
                try
                {
                    a.Invoke(client);
                }
                catch (Exception e)
                {
                    Clients.TryRemove(client.Key, out _);
                    Console.WriteLine(e);
                }
            }
        });
    }

    private async Task SaveMessage(IMessageDto textMessage, bool isPrivate = false, bool isFile = false)
    {
        await Task.Run(() =>
        {
            var db = new SQLiteConnection($"Data Source={DbPath}");
            var message = MessageDao.FromDto(textMessage);
            message.IsPrivate = isPrivate;
            message.isFile = isFile;
            db.Open();
            db.Insert(message);
            db.Close();
        });
    }

    public void SendPrivateMessage(IMessageDto textMessage)
    {
        var now = DateTime.Now;
        textMessage.DateTime = now;
        SaveMessage(textMessage, true).Wait();
        // first send message back to sender
        // only if the sender and receiver are not the same
        if (textMessage.Sender.Id != textMessage.ChatId)
        {
            try
            {
                Clients[textMessage.Sender.Id].ReceivePrivateMessage(textMessage);
            }
            catch (Exception e)
            {
                Clients.TryRemove(textMessage.ChatId, out _);
                Console.WriteLine(e);
            }
        }
        
        // send message to receiver
        // set the ChatId to the id of the sender so it shows up as the senders message
        var receiverId = textMessage.ChatId;
        textMessage.ChatId = textMessage.Sender.Id;
        try
        {
            Clients[receiverId].ReceivePrivateMessage(textMessage);
        }
        catch (Exception e)
        {
            Clients.TryRemove(receiverId, out _);
            Console.WriteLine(e);
        }
    }
    
    public IEnumerable<IMessageDto> FetchMessages(int chatRoomId)
    {
        var db = new SQLiteConnection($"Data Source={DbPath}");
        db.Open();
        // get all non private messages by chatroom id
        var messages = db.Query<MessageDao>(
            """
            select * from Messages where
            isPrivate = false and
            chatRoomId = @ChatRoomId
            """, new {ChatRoomId = chatRoomId}
        );
        db.Close();
        return messages
            .Select(message => message.ToDto(new SQLiteConnection($"Data Source={DbPath}")));
    }

    public IEnumerable<IMessageDto> FetchPrivateMessages(int user1, int user2)
    {
        var db = new SQLiteConnection($"Data Source={DbPath}");
        db.Open();
        // get all private messages that happened between the ids of user1 and user2
        var messages = db.Query<MessageDao>(
            """
                select * from Messages where
                isPrivate = 1 and
                ((senderId = @User1 and chatRoomId = @User2) or
                (senderId = @User2 and chatRoomId = @User1))
            """, new {User1 = user1, User2 = user2});
        return messages
            .Select(message => message.ToDto(new SQLiteConnection($"Data Source={DbPath}")));
    }

    public void UploadFile(byte[] bytes, string filename, string fileExtension, UserDto sender, int chatId, bool isPrivate)
    {
        var name = $"{filename}{fileExtension}";
        var suffix = 2;
        while (File.Exists(Path.Combine(UploadPath, name)))
        {
            name = $"{filename}[{suffix}]{fileExtension}";
            suffix++;
        }
        File.WriteAllBytes(Path.Combine(UploadPath, name), bytes);
        var message = new FileMessage
        {
            Sender = sender,
            Text = name,
            ChatId = chatId,
            DateTime = DateTime.Now
        };
        Task.Run(() =>
        {
            if (!isPrivate)
            {
                Broadcast(pair => pair.Value.ReceiveBroadcast(message));
            }
            else
            {
                SendPrivateMessage(message);
            }
        });
        SaveMessage(message, isPrivate, true);
    }

    public byte[] DownloadFile(string filename)
    {
        var path = Path.Combine(UploadPath, filename);
        var bytes = Array.Empty<byte>();
        if (File.Exists(path))
        {
            bytes = File.ReadAllBytes(path);
        }
        return bytes;
    }

    private bool ValidateInput(string? username, string? password)
    {
        if (username == null || password == null) return false;
        return !(username.Length < 3 || password.Length < 3);
    }
    public byte[] FetchPlugins()
    {
        Assembly currentAssembly = Assembly.GetExecutingAssembly();
        string assemblyPath = currentAssembly.Location;

        // Read the content of the assembly file into a byte array
        byte[] assemblyBytes = File.ReadAllBytes(assemblyPath);
        return assemblyBytes;
    }
}