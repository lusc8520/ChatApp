using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class PluginWindowViewModel : ObservableObject
{
    public ObservableCollection<PluginViewModel> Plugins { get; } = new();
    
    private ChatClient ChatClient { get; }

    public PluginWindowViewModel(ChatClient chatClient)
    {
        ChatClient = chatClient;
    }
    
}