using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;

public class PrivateChat : ObservableObject, IChat
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool HasFetched { get; set; }
    public ObservableCollection<MessageDto> Messages { get; } = new();
    public void SendMessage(ChatViewModel chatViewModel)
    {
        chatViewModel.ChatClient.SendPrivateMessage(
            chatViewModel.MainViewModel.User,
            Id,
            chatViewModel.MessageText
        );
    }

    public void FetchMessages(ChatViewModel chatViewModel)
    {
        chatViewModel.ChatClient.FetchMessages(this, chatViewModel.MainViewModel.User.Id);
    }
}