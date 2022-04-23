namespace Extrator.MessageContext
{
    using Newtonsoft.Json.Linq;
    using System.Threading.Tasks;

    public interface IMessage
    {
        Task SendMessage(string section, string data);
    }
}
