using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;

public class ChatRoomViewModel : ObservableObject
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ObservableCollection<Message> Messages { get; set; } = new();
}