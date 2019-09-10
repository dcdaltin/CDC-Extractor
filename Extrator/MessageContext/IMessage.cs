namespace Extrator.MessageContext
{
    using Newtonsoft.Json.Linq;

    public interface IMessage
    {
        void SendMessage(JObject data);
    }
}
