using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class RegisterViewModel(MainViewModel mvm) : ObservableObject
{
    public MainViewModel MainViewModel { get; } = mvm;
    private ChatClient ChatClient { get; } = mvm.ChatClient;
    public string Username { get; set; }
    
    [RelayCommand]
    private void Register(PasswordBox passwordBox)
    {
        ChatClient.Register(Username, passwordBox.Password);
    }
}