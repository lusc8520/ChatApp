using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class LoginViewModel(MainViewModel mvm) : ObservableObject
{
    public MainViewModel MainViewModel { get; } = mvm;
    public string Username { get; set; }
    public string ServerMessage { get; set; }

    [RelayCommand]
    private async Task Login(PasswordBox password)
    {
        // run asynch methods to prevent ui blocking
        var res = await Task.Run(() => MainViewModel.ChatClient.Login(Username, password.Password));
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