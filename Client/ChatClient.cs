using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input.StylusPlugIns;
using Windows.UI.Xaml.Controls;
using de.hsfl.vs.hul.chatApp.client.ViewModel;
using de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.client;

public class ChatClient : IChatClient
{
    public event Action<LoginResponse> LoginSuccess;
    public event Action<LoginResponse> LoginFailed;
    public event Action LogoutSuccess;
    public event Action<IMessageDto> BroadcastReceived;
    public event Action<UserDto> UserReceived; 
    public event Action<IMessageDto> PrivateMessageReceived; 
    public event Action<IMessageDto> MessageReceiving;
    public event Action<IMessageDto> MessageSending;
    public event Action GlobalChatsFetched;

    private DuplexChannelFactory<IChatService> _factory;
    private IChatService _chatService;
    public ChatClient()
    {
        var binding = new NetTcpBinding
        {
            MaxReceivedMessageSize = 5000000, // = 5mb for pdf upload
            MaxBufferSize = 5000000
        };
        _factory = new DuplexChannelFactory<IChatService>(
            new InstanceContext(this),
            binding,
            "net.tcp://localhost:9000/chatApp");
        _chatService = _factory.CreateChannel();
        
        // erste verbindung zum server aufbauen
        // ohne dies hat die erste login/register anfrage ein wenig länger gedauert
        Task.Run(() => Execute(() => _chatService.Connect()));
    }
    public void Connect()
    {
        Console.WriteLine("connected to server!");
    }

    public void ReceiveBroadcast(IMessageDto textMessage)
    {
        var msg = MessageViewModel.FromDto(textMessage);
        MessageReceiving?.Invoke(msg);
        BroadcastReceived?.Invoke(msg);
    }

    public void ReceivePrivateMessage(IMessageDto textMessage)
    {
        var msg = MessageViewModel.FromDto(textMessage);
        MessageReceiving?.Invoke(msg);
        PrivateMessageReceived?.Invoke(msg);
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

    public void Logout(int userId)
    {
        _chatService.Logout(userId);
        LogoutSuccess?.Invoke();
    }

    public void UploadFile(byte[] bytes, string filename, string fileExtension, UserDto sender, int chatId, bool isPrivate)
    {
        Execute(() => {_chatService.UploadFile(bytes, filename, fileExtension, sender, chatId, isPrivate);});
    }

    public void DownloadFile(string filename)
    {
        Task.Run(() =>
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);
            if (!File.Exists(path))
            {
                var bytes = _chatService.DownloadFile(filename);
                if (bytes.Length <= 0) return;
                File.WriteAllBytes(path, bytes);
            }
            Process.Start($"{path}");
        });
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

    public void FetchMessages(GlobalChat chat)
    {
        Task.Run(() =>
        {
            var fetchedMessages = _chatService.FetchMessages(chat.Id);
            foreach (var fetchedMessage in fetchedMessages)
            {
                var msg = MessageViewModel.FromDto(fetchedMessage);
                MessageReceiving?.Invoke(msg);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    chat.Messages.Add(msg);
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
                var msg = MessageViewModel.FromDto(fetchedMessage);
                MessageReceiving?.Invoke(msg);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    chat.Messages.Add(msg);
                });
            }
        });
    }
    
    public void FetchPluginsName(ObservableCollection<PluginViewModel> plugins)
    {
         var assemblyBytes =_chatService.FetchPlugins();
         Assembly pluginAssembly = Assembly.Load(assemblyBytes);
         foreach (var type in pluginAssembly.GetTypes())
         {
             if (typeof(IPlugin).IsAssignableFrom(type))
             {
                 plugins.Add(new PluginViewModel(type.Name, this));
             }
         }
    }
    
    public void FetchAndInstallPlugin(PluginViewModel plugin)
    {
        var assemblyBytes =_chatService.FetchPlugins();
        Assembly pluginAssembly = Assembly.Load(assemblyBytes);
        foreach (var type in pluginAssembly.GetTypes())
        {
            if (typeof(IPlugin).IsAssignableFrom(type))
            {
                if (plugin.Name == type.Name)
                {
                    var plg = Activator.CreateInstance(type) as IPlugin;
                    var result = MessageBox.Show(
                        "Are you sure you want to install " + plugin.Name + "?", 
                        "Confirmation", 
                        MessageBoxButton.YesNo, 
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        plg?.Install(this);
                        plugin.Plugin = plg;
                        MessageBox.Show(
                            plugin.Name + " is installed successfully!", 
                            "Confirmation", 
                            MessageBoxButton.OK, 
                            MessageBoxImage.Information);
                    }
                }
            }
        }
    }

    public void SendPrivateMessage(UserDto sender, int receiverId, string text)
    {
        var newMessage = new TextMessage
        {
            Sender = sender,
            ChatId = receiverId,
            Text = text
        };
        MessageSending?.Invoke(newMessage);
        _chatService.SendPrivateMessage(
            newMessage
        );
    }
    public void BroadcastMessage(UserDto sender, int chatRoomId, string text)
    {
        var newMessage = new TextMessage
        {
            Sender = sender,
            Text = text,
            ChatId = chatRoomId
        };
        MessageSending?.Invoke(newMessage);
        _chatService.BroadcastMessage(
            newMessage
        );
    }
}
