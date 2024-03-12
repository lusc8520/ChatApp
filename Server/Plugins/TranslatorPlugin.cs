using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using DeepL;

namespace de.hsfl.vs.hul.chatApp.server.Plugins;

public class TranslatorPlugin : IPlugin
{
    private const string AuthKey = "b73d3700-f1f0-4c0c-8bbc-0cbee8230cef:fx";
    private Translator _translator = null!;
    private readonly Dictionary<string, string> _languageCodeMap = new Dictionary<string, string>();
    private string _selectedLanguage = "German";
    
    public async void Install(IChatClient chatClient)
    {
        _translator = new Translator(AuthKey);
        var targetLanguages = await _translator.GetTargetLanguagesAsync();
        foreach (var lang in targetLanguages)
        {
            _languageCodeMap[lang.Name] = lang.Code;
        }
        chatClient.MessageReceiving += dto =>
        {
            Execute(dto);
        };
    }

    private async Task Execute(IMessageDto message)
    {
        var targetLanguageCode = _languageCodeMap[_selectedLanguage];
        var translatedText = await _translator.TranslateTextAsync(
            message.Text,
            null,
            targetLanguageCode);
        Console.WriteLine(translatedText.Text);
        message.Text = translatedText.Text;
    }

    public async void OpenPluginOptions()
    {
        var newWindow = new Window
        {
            Title = "Select Translation Language",
            Width = 250,
            Height = 300,
            Background = Brushes.DarkSlateGray
        };
        
        var scrollViewer = new ScrollViewer
        {
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };
        var listBox = new ListBox
        {
            SelectionMode = SelectionMode.Single,
            Margin = new Thickness(10),
            Background = Brushes.DarkCyan,
            Foreground = Brushes.White
        };
        
        // Events for selecting a new translation language
        listBox.SelectionChanged += ListBox_SelectionChanged;
        listBox.MouseDoubleClick += ListBox_MouseDoubleClick;
        
        // Add languages from the language mapping dictionary to listBox
        foreach (var lang in _languageCodeMap.Keys)
        {
            listBox.Items.Add(lang);
        }
        scrollViewer.Content = listBox;
        newWindow.Content = scrollViewer;
        newWindow.Show();
    }
    
    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        HandleSelectionChange(sender as ListBox);
    }

    private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        HandleSelectionChange(sender as ListBox);
        Window.GetWindow(sender as ListBox)?.Close();
    }

    // set the new selected language
    private void HandleSelectionChange(ListBox? listBox)
    {
        var selectedLanguage = listBox?.SelectedItem?.ToString();
        if (selectedLanguage != null)
        {
            _selectedLanguage = selectedLanguage;
        }
    }
    
}