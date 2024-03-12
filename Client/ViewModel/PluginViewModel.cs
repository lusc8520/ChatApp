using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using de.hsfl.vs.hul.chatApp.client.View;
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
    
    private bool _isPluginAvailable;
    public bool IsPluginAvailable
    {
        get => _isPluginAvailable;
        set
        {
                _isPluginAvailable = value;
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
        if (Plugin != null)
        {
            MessageBox.Show(
                Name + " is already installed!", 
                "Information", 
                MessageBoxButton.OK, 
                MessageBoxImage.Information);
        }
        else
        { 
            _chatClient.FetchAndInstallPlugin(this);
        }
    }
    
    [RelayCommand]
    private void OpenPluginOptions()
    {
            Plugin?.OpenPluginOptions();
    }
}