using System;
using System.ServiceModel;
using CommunityToolkit.Mvvm.Input;
using de.hsfl.vs.hul.chatApp.contract;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class MainViewModel : ViewModelBase
{
      private readonly LoginViewModel _loginViewModel = new();
      private readonly RegisterViewModel _registerViewModel = new();

       private ViewModelBase _currentView;

       public ViewModelBase CurrentView
       {
             get => _currentView;
             set
             {
                   _currentView = value;
                   OnPropertyChanged();
             }
       }

      public MainViewModel()
      {
            CurrentView = _loginViewModel; // set default navigation
            DuplexChannelFactory<IChatService> factory = new DuplexChannelFactory<IChatService>(
                  new InstanceContext(new ChatClient()),
                  new NetTcpBinding(),
                  "net.tcp://localhost:9000/chatApp");
            IChatService service = factory.CreateChannel();
            service.Connect();
      }

      [RelayCommand]
      private void NavigateToLogin()
      {
            CurrentView = _loginViewModel;
      }
      
      [RelayCommand]
      private void NavigateToRegister()
      {
            CurrentView = _registerViewModel;
      }
}