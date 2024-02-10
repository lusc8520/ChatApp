using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class ChatViewModel(MainViewModel mvm) : ObservableObject
{
    public MainViewModel MainViewModel { get; } = mvm;

    [RelayCommand]
    private void Logout()
    {
        Task.Run(() => { MainViewModel.ChatClient.Logout(); });
    }
}