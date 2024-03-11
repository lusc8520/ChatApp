using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Design.Serialization;
using System.Threading.Tasks;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using DeepL;
using DeepL.Model;

namespace de.hsfl.vs.hul.chatApp.server.Plugins;

public class TranslatorPlugin : IPlugin
{
    private const string AuthKey = "b73d3700-f1f0-4c0c-8bbc-0cbee8230cef:fx";
    private Translator _translator = null!;
    public string _selectedOption;
    
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
        var translatedText = await _translator.TranslateTextAsync(
            message.Text,
            null,
            _selectedOption);
        Console.WriteLine(translatedText.Text);
        message.Text = translatedText.Text;
    }

    public async Task<ObservableCollection<string>> GetPluginOptions()
    {
        var targetLanguages = await _translator.GetTargetLanguagesAsync();
        var options = new ObservableCollection<string>();
        foreach (var lang in targetLanguages) {
            options.Add(lang.Name);
            Console.WriteLine($"{lang.Name} ({lang.Code}) supports formality");
        }
        return options;
    }
    
}