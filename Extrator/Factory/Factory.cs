namespace Extrator.Factory
{
    using System;
    using Extrator.MessageContext;
    using Extrator.MessageContext.ServiceBUS;
    using Extrator.SQLContext;
    using Extrator.SQLContext.MSSQL;
    using Extrator.SQLContext.Oracle;
    using Extrator.SQLContext.Postgres;
    using Microsoft.Extensions.Configuration;

    public class Factory : IFactory
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IConfiguration config;

        public Factory(IConfiguration config)
        {
            this.config = config;
        }

        public IDatabase GetDatabase()
        {
            Logger.Debug("Getting database config...");
            var driver = config.GetSection("Driver").Value;
            if (string.IsNullOrEmpty(driver)) throw new NullReferenceException("[Driver]");
            Logger.Info($"Driver: {driver}");
            switch (driver.ToUpperInvariant())
            {
                case "MSSQL":
                    return new MSSQLContext(config);
                case "ORACLE":
                    return new OracleContext(config);
                case "POSTGRES":
                    return new PostgresContext(config);
                default:
                    {
                        Logger.Fatal($"Driver not implemented: {driver}", new NotImplementedException($"[Driver]: {driver}"));
                        throw new NotImplementedException($"[Driver]: {driver}");
                    }
            }
        }

        public IMessage GetMessagingService()
        {
            Logger.Debug("Getting messaging config...");
            var messagingService = config.GetSection("QueueService").Value;
            if (string.IsNullOrEmpty(messagingService)) throw new NullReferenceException("[QueueService]");
            switch (messagingService.ToUpperInvariant())
            {
                case "SERVICEBUS":
                    return new ServiceBUS(config);
                default:
                    {
                        Logger.Fatal($"Messaging Service not implemented: {messagingService}", new NotImplementedException($"[QueueService]: {messagingService}"));
                        throw new NotImplementedException($"[QueueService]: {messagingService}");
                    }
            }
        }
    }
}
