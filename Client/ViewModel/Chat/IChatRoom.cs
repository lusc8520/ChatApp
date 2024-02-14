using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;

public interface IChatRoom
{
    public string ChatName { get; set; }
}