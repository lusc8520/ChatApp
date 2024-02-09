using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class RegisterViewModel(MainViewModel mvm) : ObservableObject
{
    public MainViewModel MainViewModel { get; } = mvm;
    public string Username { get; set; }
    
    public string ServerMessage { get; set; }
    
    [RelayCommand]
    private async Task Register(PasswordBox passwordBox)
    {
        var res = await Task.Run(() => MainViewModel.ChatClient.Register(Username, passwordBox.Password));
        ShowMessage(res.Text);
    }
    
    private async void ShowMessage(string s)
    {
        await Task.Run(async () =>
        {
            ServerMessage = s;
            OnPropertyChanged("ServerMessage");
            await Task.Delay(2000);
            ServerMessage = "";
            OnPropertyChanged("ServerMessage");
        });
    }
}