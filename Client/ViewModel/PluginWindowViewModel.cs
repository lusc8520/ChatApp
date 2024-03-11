using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using de.hsfl.vs.hul.chatApp.contract;
using Microsoft.Extensions.Options;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class PluginWindowViewModel : ObservableObject
{
    private ObservableCollection<IPlugin> _plugins;
    public ObservableCollection<IPlugin> Plugins
    {
        get => _plugins;
        set => SetProperty(ref _plugins, value);
    } 
        
    private ObservableCollection<string> _pluginsName;
    public ObservableCollection<string> PluginsName
    {
        get => _pluginsName;
        set => SetProperty(ref _pluginsName, value);
    }
    
    private ObservableCollection<string> _pluginOptions;
    public ObservableCollection<string> PluginOptions
    {
        get => _pluginOptions;
        set => SetProperty(ref _pluginOptions, value);
    }
    
    private ChatClient ChatClient { get; }

    public PluginWindowViewModel(ChatClient chatClient)
    {
        ChatClient = chatClient;
        PluginsName = new ObservableCollection<string>();
        Plugins = new ObservableCollection<IPlugin>();
    }

    [RelayCommand]
    private void InstallPlugin(string pluginName)
    {
        ChatClient.InstallPlugin(pluginName, Plugins, this);
    }
}