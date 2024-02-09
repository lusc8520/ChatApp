using System;
using System.ServiceModel;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.client
{
    public class ChatClient : IChatClient
    {
        private readonly IChatService _chatService;
        public ChatClient()
        {
            var factory = new DuplexChannelFactory<IChatService>(
                new InstanceContext(this),
                new NetTcpBinding(),
                "net.tcp://localhost:9000/chatApp");
            _chatService = factory.CreateChannel();
            _chatService.Connect();
        }
        public void Receive(string s)
        {
            Console.WriteLine($"Received Message: {s}");
        }

        public LoginResponse Login(string username, string password)
        {
            return _chatService.Login(username, password);
        }

        public LoginResponse Register(string username, string password)
        {
            return _chatService.Register(username, password);
        }
    }
}