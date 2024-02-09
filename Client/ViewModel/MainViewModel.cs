using System;
using System.ServiceModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using de.hsfl.vs.hul.chatApp.contract;

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
                   if (value == _currentView) return;
                   _currentView = value;
                   OnPropertyChanged();
             }
       }

      public MainViewModel()
      {
            LoginViewModel = new LoginViewModel(this);
            RegisterViewModel = new RegisterViewModel(this);
            ChatViewModel = new ChatViewModel(this);
            ChatClient = new ChatClient();
            CurrentView = LoginViewModel; // set default navigation
            
            // connection to server
            // TODO irgendwo hin auslagern ?
            // DuplexChannelFactory<IChatService> factory = new DuplexChannelFactory<IChatService>(
            //       new InstanceContext(new ChatClient()),
            //       new NetTcpBinding(),
            //       "net.tcp://localhost:9000/chatApp");
            // IChatService service = factory.CreateChannel();
            // service.Connect();
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
}