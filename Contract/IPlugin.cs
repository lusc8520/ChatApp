using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace de.hsfl.vs.hul.chatApp.contract;

public interface IPlugin
{
    void Install(IChatClient icChatClient);
    Task<ObservableCollection<string>> GetPluginOptions();
}