using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using DeepL;
using DeepL.Model;

namespace de.hsfl.vs.hul.chatApp.server.Plugins;

public class TranslatorPlugin : IPlugin
{
    private const string AuthKey = "b73d3700-f1f0-4c0c-8bbc-0cbee8230cef:fx";
    private Translator _translator = null!;
    private readonly Dictionary<string, string> _languageCodeMap = new Dictionary<string, string>();
    private string _selectedLanguage;
    
    public void Install(IChatClient chatClient)
    {
        _translator = new(AuthKey);
        chatClient.MessageReceiving += dto =>
        {
            Execute(dto);
        };
        Console.WriteLine("Translator");
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
        Console.WriteLine("Translator: " + message.Text);
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
        
        listBox.SelectionChanged += ListBox_SelectionChanged;
        listBox.MouseDoubleClick += ListBox_MouseDoubleClick;

        var targetLanguages = await _translator.GetTargetLanguagesAsync();
        foreach (var lang in targetLanguages)
        {
            _languageCodeMap[lang.Name] = lang.Code;
            listBox.Items.Add(lang.Name);
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

    private void HandleSelectionChange(ListBox? listBox)
    {
        var selectedLanguage = listBox?.SelectedItem?.ToString();
        if (selectedLanguage != null)
        {
            _selectedLanguage = selectedLanguage;
            Console.WriteLine($"Selected Language: {_selectedLanguage}");
        }
    }
    
}