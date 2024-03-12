using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows;
using de.hsfl.vs.hul.chatApp.client.ViewModel;
using de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using Serilog;

namespace de.hsfl.vs.hul.chatApp.client;

public class ChatClient : IChatClient
{
    public event Action<LoginResponse> LoginSuccess;
    public event Action<LoginResponse> LoginFailed;
    public event Action RequestFailed;
    public event Action LogoutSuccess;

    public event Action Connecting;
    public event Action<IMessageDto> BroadcastReceived;
    public event Action<UserDto> UserReceived; 
    public event Action<IMessageDto> PrivateMessageReceived; 
    public event Action<IMessageDto> MessageReceiving;
    public event Action<IMessageDto> MessageSending;
    public event Action GlobalChatsFetched;
    
    private IChatService _chatService;
    public ChatClient()
    {
        var binding = new NetTcpBinding
        {
            MaxReceivedMessageSize = 5000000, // = 5mb for pdf download
            MaxBufferSize = 5000000
        };
        var factory = new DuplexChannelFactory<IChatService>(
            new InstanceContext(this),
            binding,
            "net.tcp://localhost:9000/chatApp");
        _chatService = factory.CreateChannel();
        
        // erste verbindung zum server aufbauen
        // ohne dies hat die erste login/register anfrage ein wenig länger gedauert
        Execute(() => _chatService.Connect());
    }
    public void Connect()
    {
        Console.WriteLine("connected to server!");
    }

    public void ReceiveBroadcast(IMessageDto textMessage)
    {
        Log.Information($"Received Broadcast. Message ID: {textMessage.Id}");
        var msg = MessageViewModel.FromDto(textMessage);
        MessageReceiving?.Invoke(msg);
        BroadcastReceived?.Invoke(msg);
    }

    public void ReceivePrivateMessage(IMessageDto textMessage)
    {
        Log.Information($"Received Broadcast. Message ID: {textMessage.Id}");
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
        Execute(() =>
        {
            Log.Information($"Login: Username: {username}");
            HandleLoginResponse(_chatService.Login(username, password));
        });
    }

    public void Register(string username, string password)
    {
        Execute(() =>
        {
            Log.Information($"Register: Username: {username}");
            HandleLoginResponse(_chatService.Register(username, password));
        });
    }

    private void HandleLoginResponse(LoginResponse response)
    {
        if (response.UserDto != null)
        {
            LoginSuccess?.Invoke(response);
            Log.Information($"Login Success: username:{response.UserDto.Username} userID: {response.UserDto.Id}");
            return;
        }
        Log.Information($"Login Failed: {response.Text}");
        LoginFailed?.Invoke(response);
    }

    private void Execute(Action action)
    {
        Task.Run(() =>
        {
            try
            {
                action.Invoke();
                Log.Information($"Method Name: {action.Method}, was successful");
            }
            catch (Exception e)
            {
                Connecting?.Invoke();
                Log.Warning($"Method Name: {action.Method}, was unsuccessful, trying again with new connection" +
                            $" ... Exception: {e.Message}");
                var binding = new NetTcpBinding
                {
                    MaxReceivedMessageSize = 5000000, // = 5mb for pdf download
                    MaxBufferSize = 5000000
                };
                var factory = new DuplexChannelFactory<IChatService>(
                    new InstanceContext(this),
                    binding,
                    "net.tcp://localhost:9000/chatApp");
                _chatService = factory.CreateChannel();
                try
                {
                    action.Invoke();
                    Log.Information($"Method Name: {action.Method}, was successful with new connection");
                }
                catch (Exception e2)
                {
                    Log.Error($"Method Name: {action.Method}, was unsuccessful, logging out ..." +
                              $" Exception: {e2.Message}");
                    Console.WriteLine(e2);
                    RequestFailed?.Invoke();
                }
            }
        });
        
    }

    public void Logout(int userId)
    {
        Execute(() =>
        {
            Log.Information($"Logout. userID: {userId}");
            _chatService.Logout(userId);
            LogoutSuccess?.Invoke();
        });
    }

    public void UploadFile(byte[] bytes, string filename, string fileExtension, UserDto sender, int chatId, bool isPrivate)
    {
        Execute(() =>
        {
            Log.Information($"File Upload. file name: {filename}{fileExtension}, userID: {sender.Id}, chatID: {chatId}" +
                            $"private: {isPrivate}");
            _chatService.UploadFile(bytes, filename, fileExtension, sender, chatId, isPrivate);
        });
    }

    public void DownloadFile(string filename)
    {
        Execute(() =>
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), filename);
            if (!File.Exists(path))
            {
                Log.Information($"File Download. file name: {filename}");
                var bytes = _chatService.DownloadFile(filename);
                if (bytes.Length <= 0) return;
                File.WriteAllBytes(path, bytes);
            }
            Process.Start($"{path}");
        });
    }

    public void FetchChats(ObservableCollection<GlobalChat> chats)
    {
        Execute(() =>
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
        Execute(() =>
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
        Execute(() =>
        {
            Log.Information($"Fetch Global Chat Messages. ChatID: {chat.Id}");
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
        Execute(() =>
        {
            Log.Information($"Fetch Private Chat Messages. requesterID: {userId}, ChatID: {chat.Id}");
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
        Execute(() =>
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
        });
    }
    
    public void FetchAndInstallPlugin(PluginViewModel plugin)
    {
        Execute(() =>
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
                            Log.Information($"Install Plugin: {plugin.Name}");
                            MessageBox.Show(
                                plugin.Name + " is installed successfully!", 
                                "Confirmation", 
                                MessageBoxButton.OK, 
                                MessageBoxImage.Information);
                        }
                    }
                }
            }
        });
    }

    public void SendPrivateMessage(UserDto sender, int receiverId, string text)
    {
        Execute(() =>
        {
            Log.Information($"Private Message. sender: {sender.Id}, receiverID: {receiverId}");
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
        });
    }
    public void BroadcastMessage(UserDto sender, int chatRoomId, string text)
    {
        Execute(() =>
        {
            Log.Information($"Broadcast Message. sender: {sender.Id}, chatID: {chatRoomId}");
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
        });
    }
}
