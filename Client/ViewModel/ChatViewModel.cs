using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;
using de.hsfl.vs.hul.chatApp.contract;
using Microsoft.Win32;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class ChatViewModel : ObservableObject
{
    public ChatClient ChatClient { get; }
    public MainViewModel MainViewModel { get; }
    public ObservableCollection<GlobalChat> GlobalChats { get; set; } = new();
    public ObservableCollection<PrivateChat> PrivateChats { get; set; } = new();
    public ObservableCollection<IPlugin> Plugins { get; set; } = new();
    public IPlugin Plugin { get; set; }
    
    private IChat _selectedChat;
    public IChat SelectedChat
    {
        get => _selectedChat;
        set
        {
            // set it to null first because this property is bound to 2 list views.
            // this ensures that an item is de-selected from listview1 before selecting something in listview2.
            _selectedChat = null;
            OnPropertyChanged();
            _selectedChat = value;
            OnPropertyChanged();
            if (_selectedChat == null) return;
            if (_selectedChat.HasFetched) return; // only fetch messages once
            _selectedChat.HasFetched = true;
            _selectedChat.FetchMessages(this);
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
    
    private bool ChatsFetched { get; set; }
    public ChatViewModel(MainViewModel mvm)
    {
        MainViewModel = mvm;
        ChatClient = MainViewModel.ChatClient;
        // set up client events
        ChatClient.UserReceived += user =>
        {
            Console.WriteLine(user.Username);
            PrivateChats.Add(new PrivateChat
            {
                Id = user.Id,
                Name = user.Username
            });
        };
        ChatClient.GlobalChatsFetched += () =>
        {
            SelectedChat = GlobalChats.FirstOrDefault();
        };
        ChatClient.LoginSuccess += _ =>
        {
            if (ChatsFetched) return;
            ChatClient.FetchChats(PrivateChats);
            ChatClient.FetchChats(GlobalChats);
            ChatsFetched = true;
        };
        ChatClient.BroadcastReceived += message =>
        {
            var chat = GlobalChats.SingleOrDefault(chat => chat.Id == message.ChatId);
            if (chat == null) return;
            if (!chat.HasFetched) return;
            chat.Messages.Add(message);
            Console.WriteLine("ViewModel: " + message.Text);
        };
        ChatClient.PrivateMessageReceived += message =>
        {
            var chat = PrivateChats.SingleOrDefault(chat => chat.Id == message.ChatId);
            if (chat == null) return;
            if (!chat.HasFetched) return;
            chat.Messages.Add(message);
        };
    }

    [RelayCommand]
    private void Logout()
    {
        ChatClient.Logout();
    }

    [RelayCommand]
    private void SendMessage()
    {
        if (string.IsNullOrEmpty(MessageText)) return;
        SelectedChat?.SendMessage(this);
        MessageText = "";
    }

    [RelayCommand]
    private void ChooseFile()
    {
        var ofd = new OpenFileDialog
        {
            DefaultExt = ".pdf",
            Filter = "PDF Files (*.pdf)|*.pdf"
        };
        var result = ofd.ShowDialog();
        if (result ?? false)
        {
            var stream = ofd.OpenFile();
            var bytes = new byte[stream.Length];
            var k = stream.Read(bytes, 0, (int)stream.Length);
            ChatClient.UploadPdf(bytes);
        }
    }

    [RelayCommand]
    private void InstallPlugin()
    {
        ChatClient.FetchPlugins(Plugin);
    }
}