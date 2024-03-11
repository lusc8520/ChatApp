using System.Collections.ObjectModel;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;

public interface IChat
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ObservableCollection<IMessageDto> Messages { get; }
    public void SendMessage(ChatViewModel chatViewModel);
    public void FetchMessages(ChatViewModel chatViewModel);
    public bool HasFetched { get; set; }
}