using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class ChatViewModel(MainViewModel mvm) : ObservableObject
{
    public MainViewModel MainViewModel { get; } = mvm;
    

    public ObservableCollection<IChatRoom> Chats { get; } = new()
    {
        new GroupChatViewModel { ChatName = "Global Chat" },
        new GroupChatViewModel { ChatName = "Global Chat" },
        new GroupChatViewModel { ChatName = "Global Chat" },
        new GroupChatViewModel { ChatName = "Global Chat" },
        new GroupChatViewModel { ChatName = "Global Chat" },
        new GroupChatViewModel { ChatName = "Global Chat" },
        new GroupChatViewModel { ChatName = "Global Chat" },
        new GroupChatViewModel { ChatName = "Global Chat" },
    };

    [RelayCommand]
    private void Logout()
    {
        Task.Run(() => { MainViewModel.ChatClient.Logout(); });
    }
}