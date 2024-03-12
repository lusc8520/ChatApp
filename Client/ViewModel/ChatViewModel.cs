using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using de.hsfl.vs.hul.chatApp.client.View;
using de.hsfl.vs.hul.chatApp.client.ViewModel.Chat;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using Microsoft.Toolkit.Uwp.Notifications;
using Microsoft.Win32;

namespace de.hsfl.vs.hul.chatApp.client.ViewModel;

public partial class ChatViewModel : ObservableObject
{
    public ChatClient ChatClient { get; }
    public MainViewModel MainViewModel { get; }
    public ObservableCollection<GlobalChat> GlobalChats { get; set; } = new();
    public ObservableCollection<PrivateChat> PrivateChats { get; set; } = new();
    
    private readonly PluginWindowViewModel _pluginWindowViewModel;
    
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
        _pluginWindowViewModel = mvm.PluginWindowViewModel;
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
            ChatClient.FetchPluginsName(_pluginWindowViewModel.Plugins);
            ChatsFetched = true;
        };
        ChatClient.BroadcastReceived += message =>
        {
            var chat = GlobalChats.SingleOrDefault(chat => chat.Id == message.ChatId);
            if (chat == null) return;
            if (!chat.HasFetched) return;
            chat.Messages.Add(message);
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
        ChatClient.Logout(MainViewModel.User.Id);
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
            Filter = "PDF Files (*.pdf)|*.pdf" +
                     "|PNG Files (*.png)|*.png" +
                     "|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg" +
                     "|Text Files (*.txt)|*.txt"
        };
        var result = ofd.ShowDialog();
        if (result ?? false)
        {
            var stream = ofd.OpenFile();
            if (stream.Length > 5000000)
            {
                new ToastContentBuilder()
                    .AddText("file is too large (max is 5mb)")
                    .Show();
                return;
            }
            var bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            
            ChatClient.UploadFile(bytes,
                Path.GetFileNameWithoutExtension(ofd.FileName), 
                Path.GetExtension(ofd.FileName),
                MainViewModel.User,
                _selectedChat.Id,
                _selectedChat is PrivateChat
            );
        }
    }

    [RelayCommand]
    private void DownloadFile(string filename)
    {
        ChatClient.DownloadFile(filename);
    }

    [RelayCommand]
    private void OpenPluginWindow()
    {
        var pluginWindow = new PluginWindow
        {
            DataContext = _pluginWindowViewModel
        };
        pluginWindow.Show();
    }
}