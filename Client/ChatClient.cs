using System;
using System.ServiceModel;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.client;

public class ChatClient : IChatClient
{
    public event Action<LoginResponse> LoginSuccess;
    public event Action<LoginResponse> LoginFailed;
    public event Action LogoutSucces;
    
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
        _chatService.Connect();
    }
    public void Connect()
    {
        Console.WriteLine("connected to server!");
    }

    public void Login(string username, string password)
    {
        Execute(() =>
        {
            HandleLoginResponse(_chatService.Login(username, password));
        });
    }

    public void Register(string username, string password)
    {
        Execute(() =>
        {
            HandleLoginResponse(_chatService.Register(username, password));
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
            action();
        }
        catch
        {
            // try again with new server connection
            var factory = new DuplexChannelFactory<IChatService>(
                new InstanceContext(this),
                new NetTcpBinding(),
                "net.tcp://localhost:9000/chatApp");
            _chatService = factory.CreateChannel();
            action();
            // TODO: show (server?) error if it fails again ?
        }
    }

    public void Logout()
    {
        // TODO send logout message to server ?
        LogoutSucces?.Invoke();
    }
}
