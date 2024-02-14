using CommunityToolkit.Mvvm.ComponentModel;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;

public class PrivateChatViewModel : IChatRoom
{
    public string ChatName { get; set; }
}