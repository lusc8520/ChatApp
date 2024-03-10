using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.client;

public class ChatClient : IChatClient
{
    public event Action<LoginResponse> LoginSuccess;
    public event Action<LoginResponse> LoginFailed;
    public event Action LogoutSuccess;
    public event Action<MessageDto> BroadcastReceived;
    public event Action<UserDto> UserReceived; 
    public event Action<MessageDto> PrivateMessageReceived; 
    public event Action GlobalChatsFetched;
    public event Action<MessageDto> MessageReceiving;
    
    private IChatService _chatService;
    public ChatClient()
    {
        var factory = new DuplexChannelFactory<IChatService>(
            new InstanceContext(this),
            new NetTcpBinding(),
            "net.tcp://localhost:9000/chatApp");
        _chatService = factory.CreateChannel();
        
        // erste verbindung zum server aufbauen
        // ohne dies hat die erste login/register anfrage ein wenig länger gedauert
        Task.Run(() => Execute(() => _chatService.Connect()));
    }
    public void Connect()
    {
        Console.WriteLine("connected to server!");
    }

    public void ReceiveBroadcast(MessageDto messageDto)
    {
        MessageReceiving?.Invoke(messageDto);  
        Console.WriteLine("Test: " + messageDto.Text);
        BroadcastReceived?.Invoke(messageDto);
    }

    public void ReceivePrivateMessage(MessageDto messageDto)
    {
        MessageReceiving?.Invoke(messageDto);
        PrivateMessageReceived?.Invoke(messageDto);
    }

    public void ReceiveNewUser(UserDto user)
    {
        UserReceived?.Invoke(user);
    }

    public void Login(string username, string password)
    {
        Task.Run(() =>
        {
            Execute(() =>
            {
                HandleLoginResponse(_chatService.Login(username, password));
            });
        });
    }

    public void Register(string username, string password)
    {
        Task.Run(() =>
        {
            Execute(() =>
            {
                HandleLoginResponse(_chatService.Register(username, password));
            });
        });
    }

    private void HandleLoginResponse(LoginResponse response)
    {
        if (response.UserDto != null)
        {
            LoginSuccess?.Invoke(response);
            return;
        }
        LoginFailed?.Invoke(response);
    }

    private void Execute(Action action)
    {
        // try to execute method
        try
        {
            action.Invoke();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            LoginFailed?.Invoke( new LoginResponse
            {
                Text = "trying to connect to server..."
            });
            // try again with new server connection
            var factory = new DuplexChannelFactory<IChatService>(
                new InstanceContext(this),
                new NetTcpBinding(),
                "net.tcp://localhost:9000/chatApp");
            _chatService = factory.CreateChannel();
            try
            {
                action.Invoke();
            }
            catch (Exception e2)
            {
                Console.WriteLine(e2);
                // if it still does not work, show error message
                LoginFailed?.Invoke(new LoginResponse
                {
                    Text = "the server is not running"
                });
            }
        }
    }

    public void Logout()
    {
        // TODO send logout message to server ?
        LogoutSuccess?.Invoke();
    }

    public void UploadPdf(byte[] bytes)
    {
        Console.WriteLine("huh");
        _chatService.UploadPdf(bytes);
    }

    public void FetchChats(ObservableCollection<GlobalChat> chats)
    {
        Task.Run(() =>
        {
            var fetchedChats = _chatService.FetchChatRooms();
            foreach (var chatRoom in fetchedChats)
            {
                // invoke the following from the application dispatcher
                // because this runs in a background thread while the currentChatList is on the main thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    chats.Add(new GlobalChat()
                    {
                        Id = chatRoom.Id,
                        Name = chatRoom.Name
                    });
                });
            }
            GlobalChatsFetched?.Invoke();
        });
    }

    public void FetchChats(ObservableCollection<PrivateChat> chats)
    {
        Task.Run(() =>
        {
            var fetchedChats = _chatService.FetchUsers();
            foreach (var user in fetchedChats)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    chats.Add(new PrivateChat
                    {
                        Name = user.Username,
                        Id = user.Id
                    });
                });
            }
        });
    }

    public void FetchPlugins(IPlugin plugins)
    {
        var assemblyBytes =_chatService.FetchPlugins();
        Assembly pluginAssembly = Assembly.Load(assemblyBytes);
        foreach (var type in pluginAssembly.GetTypes())
        {
            if (typeof(IPlugin).IsAssignableFrom(type))
            {
                var plugin = Activator.CreateInstance(type) as IPlugin;
                if (plugin != null)
                {
                    plugins = plugin;
                    //plugins.Add(plugin);
                    Console.WriteLine($"Plugin: {plugins.GetType().Name}");
                    plugin.Install(this);
                }
            }
        }
        //var translatorPlugin = plugins.First();
    }

    public void BroadcastMessage(UserDto sender, int chatRoomId, string text)
    {
        _chatService.BroadcastMessage(
            new MessageDto
            {
                Sender = sender,
                Text = text,
                ChatId = chatRoomId
            }
        );
    }

    public void FetchMessages(GlobalChat chat)
    {
        Task.Run(() =>
        {
            var fetchedMessages = _chatService.FetchMessages(chat.Id);
            foreach (var fetchedMessage in fetchedMessages)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    chat.Messages.Add(fetchedMessage);
                });
            }
        });
    }

    public void FetchMessages(PrivateChat chat, int userId)
    {
        Task.Run(() =>
        {
            var fetchedMessages = _chatService.FetchPrivateMessages(userId, chat.Id);
            foreach (var fetchedMessage in fetchedMessages)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    chat.Messages.Add(fetchedMessage);
                });
            }
        });
    }

    public void SendPrivateMessage(UserDto sender, int receiverId, string text)
    {
        _chatService.SendPrivateMessage(
            new MessageDto
            {
                Sender = sender,
                ChatId = receiverId,
                Text = text
            }
        );
    }
}
