using System;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class MainViewModel : ObservableObject
{
      public LoginViewModel LoginViewModel { get; }
      public RegisterViewModel RegisterViewModel { get; }
      public ChatViewModel ChatViewModel { get; }
      public ChatClient ChatClient { get; }
      
      private ObservableObject _currentView;
       public ObservableObject CurrentView
       {
             get => _currentView;
             set
             {
                   _currentView = value;
                   ServerMessage = "";
                   OnPropertyChanged();
             }
       }

       private string _serverMessage;
       public string ServerMessage
       {
             get => _serverMessage;
             set
             {
                   _serverMessage = value;
                   OnPropertyChanged();
             }
       }

       private User _user;

       public User User
       {
             get => _user;
             set
             {
                   _user = value;
                   OnPropertyChanged();
             }
       }

      public MainViewModel()
      {
            ChatClient = new ChatClient();
            // set up client events
            ChatClient.LoginSuccess += response =>
            {
                  User = response.User;
                  NavigateToChat();
            };
            ChatClient.LoginFailed += response =>
            {
                  ShowMessage(response.Text);
            };
            ChatClient.LogoutSucces += () =>
            {
                  User = null;
                  NavigateToLogin();
            };
            // initialize other view models
            LoginViewModel = new LoginViewModel(this);
            RegisterViewModel = new RegisterViewModel(this);
            ChatViewModel = new ChatViewModel(this);
            // set default navigation
            CurrentView = LoginViewModel;
      }

      [RelayCommand]
      private void NavigateToLogin()
      {
            CurrentView = LoginViewModel;
      }
      
      [RelayCommand]
      private void NavigateToRegister()
      {
            CurrentView = RegisterViewModel;
      }

      [RelayCommand]
      private void NavigateToChat()
      {
            CurrentView = ChatViewModel;
      }

      private async void ShowMessage(string s)
      {
            // show message for 2 seconds then hide it
            ServerMessage = s;
            await Task.Delay(2000);
            ServerMessage = "";
      }
}