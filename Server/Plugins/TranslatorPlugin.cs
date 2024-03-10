using System;
using System.ServiceModel;
using System.Threading.Tasks;
using de.hsfl.vs.hul.chatApp.contract;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using DeepL;
using DeepL.Model;

namespace de.hsfl.vs.hul.chatApp.server.Plugins;

public class TranslatorPlugin : IPlugin
{
    public void Install(IChatClient ichatClient)
    {
        ichatClient.MessageReceiving += dto => 
        {
            Execute(dto, LanguageCode.German);
        };
        Console.WriteLine("Translator");
    }
    
    public void Execute(MessageDto message, string targetLanguageCode)
    {
        var authKey = "b73d3700-f1f0-4c0c-8bbc-0cbee8230cef:fx";
        var translator = new Translator(authKey);
        var translatedText = translator.TranslateTextAsync(
            message.Text,
            null,
            LanguageCode.German);
        Console.WriteLine(translatedText);
        message.Text = translatedText.ToString();
    }
}