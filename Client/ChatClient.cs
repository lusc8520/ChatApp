using System;
using System.Collections.ObjectModel;
using System.Linq;
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
    public event Action<Message> MessageReceived;
    
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

    public void ReceiveMessage(Message message)
    {
        MessageReceived?.Invoke(message);
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
        if (response.User != null)
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
        Task.Run(() =>
        {
            // TODO send logout message to server ?
            LogoutSuccess?.Invoke();
        });
    }

    public void FetchChatRooms(ObservableCollection<ChatRoomViewModel> currentChatList)
    {
        Task.Run(() =>
        {
            var rooms = _chatService.FetchChatRooms();
            // check if fetched chatrooms already exist
            // this only works because of overriding the Equals/Hashcode methods in ChatRoom
            var newRooms = rooms.Except(
                currentChatList.Select(c => new ChatRoom { Id = c.Id, Name = c.Name })
            );
            foreach (var chatRoom in newRooms)
            {
                // invoke the following from the application dispatcher
                // because this runs in a background thread while the currentChatList is on the main thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    currentChatList.Add(new ChatRoomViewModel()
                    {
                        Id = chatRoom.Id,
                        Name = chatRoom.Name
                    });
                });
            }
        });
    }

    public void FetchUsers(ObservableCollection<User> currentUserList)
    {
        Task.Run(() =>
        {
            var fetchedUsers = _chatService.FetchUsers();
            var newUsers = fetchedUsers.Except(currentUserList);
            foreach (var user in newUsers)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    currentUserList.Add(user);
                });
            }
        });
    }

    public void SendMessage(User sender, ChatRoomViewModel chatRoom, string text)
    {
        Task.Run(() =>
        {
            _chatService.SendMessage(
                new Message
                {
                    Sender = sender,
                    Text = text,
                    ChatRoom = new ChatRoom { Id = chatRoom.Id }
                }
            );
        });
    }
}
