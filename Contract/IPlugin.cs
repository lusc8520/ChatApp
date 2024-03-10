using System.Runtime.Serialization;
using System.Threading.Tasks;
using de.hsfl.vs.hul.chatApp.contract.DTO;
using DeepL.Model;

namespace de.hsfl.vs.hul.chatApp.contract;

public interface IPlugin
{
    void Install(IChatClient icChatClient);
}