using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using de.hsfl.vs.hul.chatApp.client.Navigation;
using de.hsfl.vs.hul.chatApp.contract;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;


public partial class MainViewModel : ViewModelBase
{
      [ObservableProperty] private string _testString;

      // [ObservableProperty]
      // private ViewModelBase _currentView;
      //public Navigator Navigator => Navigator.Instance;

      public MainViewModel()
      {
            DuplexChannelFactory<IChatService> factory = new DuplexChannelFactory<IChatService>(
                  new InstanceContext(new ChatClient()),
                  new NetTcpBinding(),
                  "net.tcp://localhost:9000/chatApp");
            IChatService service = factory.CreateChannel();
            service.Connect();
      }

      // [RelayCommand]
      // private void NavigateToLogin()
      // {
      //       //Navigator.CurrentView = ViewModelLocator.LoginViewModel;
      //       //CurrentView = ViewModelLocator.LoginViewModel;
      //       CurrentView = ViewModelLocator.LoginViewModel;
      // }
      
      // [RelayCommand]
      // private void NavigateToRegister()
      // {
      //       //Navigator.CurrentView = ViewModelLocator.RegisterViewModel;
      //       //CurrentView = ViewModelLocator.RegisterViewModel;
      //       CurrentView = ViewModelLocator.RegisterViewModel;
      // }
}