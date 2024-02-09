using CommunityToolkit.Mvvm.ComponentModel;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public class ChatViewModel(MainViewModel mvm) : ObservableObject
{
    public MainViewModel MainViewModel { get; } = mvm;
}