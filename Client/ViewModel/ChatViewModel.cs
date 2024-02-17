using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;
using de.hsfl.vs.hul.chatApp.contract.DTO;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class ChatViewModel : ObservableObject
{
    public MainViewModel MainViewModel { get; }
    public ObservableCollection<ChatRoomViewModel> Chats { get; set; } = new();
    public ObservableCollection<User> UserList { get; set; } = new();
    
    private ChatRoomViewModel _selectedChat;
    public ChatRoomViewModel SelectedChat
    {
        get => _selectedChat;
        set
        {
            _selectedChat = value;
            OnPropertyChanged();
            // TODO fetch messages ?
        }
    }

    private string _messageText;
    public string MessageText
    {
        get => _messageText;
        set
        {
            _messageText = value;
            OnPropertyChanged();
        }
    }

    public ChatViewModel(MainViewModel mvm)
    {
        MainViewModel = mvm;
        // set up client events
        MainViewModel.ChatClient.LoginSuccess += _ =>
        {
            MainViewModel.ChatClient.FetchUsers(UserList);
            MainViewModel.ChatClient.FetchChatRooms(Chats);
        };
        MainViewModel.ChatClient.MessageReceived += message =>
        {
            var chat = Chats.FirstOrDefault(model => model.Id == message.ChatRoom.Id);
            if (chat == null) return;
            chat.Messages.Add(message);
        };
    }

    [RelayCommand]
    private void Logout()
    {
        MainViewModel.ChatClient.Logout();
    }

    [RelayCommand]
    private void SendMessage()
    {
        if (string.IsNullOrEmpty(MessageText)) return;
        if (SelectedChat == null) return;
        MainViewModel.ChatClient.SendMessage(MainViewModel.User, SelectedChat, MessageText);
    }
}