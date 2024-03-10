using System;
using System.ServiceModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

      private UserDto _user;

      public UserDto User
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
                  User = response.UserDto;
                  NavigateToChat();
            };
            ChatClient.LoginFailed += response =>
            {
                  if (CurrentView == ChatViewModel) NavigateToLogin();
                  ShowMessage(response.Text);
            };
            ChatClient.LogoutSuccess += () =>
            {
                  User = null;
                  NavigateToLogin();
            };
            // inject dependencies into the other view models
            LoginViewModel = new LoginViewModel(this);
            RegisterViewModel = new RegisterViewModel(this);
            ChatViewModel = new ChatViewModel(this);
            // set default navigation
            CurrentView = LoginViewModel;
            
            // login instantly for testing
            // ChatClient.Login("user", "user");
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

      private async Task ShowMessage(string s)
      {
            // show message for 2 seconds then hide it
            ServerMessage = s;
            await Task.Delay(2000);
            ServerMessage = "";
      }
}