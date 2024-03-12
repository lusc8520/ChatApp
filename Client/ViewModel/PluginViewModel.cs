using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using de.hsfl.vs.hul.chatApp.contract;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class PluginViewModel : ObservableObject
{
    private string _name;
    public string Name
    {
        get => _name;
        private set
        {
            _name = value;
            OnPropertyChanged();
        }
    }
    
    public IPlugin Plugin;
    private readonly ChatClient _chatClient;
    public PluginViewModel(string name, ChatClient chatClient)
    {
        Name = name;
        _chatClient = chatClient;
    } 
    
    [RelayCommand]
    private void FetchAndInstallPlugin()
    {
        _chatClient.FetchAndInstallPlugin(this);
    }
    
    [RelayCommand]
    private void OpenPluginOptions()
    {
        Plugin?.OpenPluginOptions();
    }

}