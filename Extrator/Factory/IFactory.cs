namespace Extrator.Factory
{
    using Extrator.MessageContext;
    using Extrator.SQLContext;

    public interface IFactory
    {
        IDatabase GetDatabase();
        IMessage GetMessagingService();
    }
}
