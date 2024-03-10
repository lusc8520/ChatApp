using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class LoginViewModel(MainViewModel mvm) : ObservableObject
{
    public MainViewModel MainViewModel { get; } = mvm;
    private ChatClient ChatClient { get; } = mvm.ChatClient;
    public string Username { get; set; }
    
    [RelayCommand]
    private void Login(PasswordBox passwordBox)
    {
        ChatClient.Login(Username, passwordBox.Password);
    }
}