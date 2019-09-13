namespace Extrator.Factory
{
    using System;
    using System.IO;
    using System.Linq;
    using Extrator.MessageContext;
    using Extrator.MessageContext.ServiceBUS;
    using Extrator.SQLContext;
    using Extrator.SQLContext.MSSQL;
    using Extrator.SQLContext.Oracle;
    using Extrator.SQLContext.Postgres;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

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
            var module = config.GetSection("Module").Value;
            if (string.IsNullOrEmpty(module)) throw new NullReferenceException("[Module]");
            switch (module.ToUpperInvariant())
            {
                case "CDC":
                    {
                        var driver = config.GetSection("CDC").GetSection("Driver").Value;
                        if (string.IsNullOrEmpty(driver)) throw new NullReferenceException("[Driver]");
                        Logger.Info($"Driver: {driver}");

                        switch (driver.ToUpperInvariant())
                        {
                            case "ORACLE":
                                return new OracleCDC(config);
                            case "POSTGRES":
                                return new PostgresCDC(config);
                            default:
                                {
                                    Logger.Fatal($"Driver {driver} not implemented for module {module}", new NotImplementedException($"[Module]: {module}, [Driver]: {driver}"));
                                    throw new NotImplementedException($"[Module]: {module}, [Driver]: {driver}");
                                }
                        }
                    }
                case "ALL":
                    {
                        var driver = config.GetSection("ALL").GetSection("Driver").Value;
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
                                    Logger.Fatal($"Driver {driver} not implemented for module {module}", new NotImplementedException($"[Module]: {module}, [Driver]: {driver}"));
                                    throw new NotImplementedException($"[Module]: {module}, [Driver]: {driver}");
                                }
                        }
                    }
                default:
                    {
                        Logger.Fatal($"Module not implemented: {module}", new NotImplementedException($"[Module]: {module}"));
                        throw new NotImplementedException($"[Module]: {module}");
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
