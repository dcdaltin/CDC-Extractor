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

        public void BuildOperationalDataFile()
        {
            var sections = config.GetSection("ListenedTables").GetChildren();
            if (!sections.Any()) throw new NullReferenceException("[ListenedTables]");
            var hasSameSections = !config.GetSection("Queries").GetChildren().Where(a => !sections.Select(b => b.Key).Contains(a.Key)).Any();
            if (!hasSameSections) throw new InvalidDataException("[ListenedTables] and [Queries] fields do not match");
            var json = new JObject();
            var tables = sections.Select(a => a.GetChildren().Select(b => b.Value));
            foreach (var list in tables)
            {
                foreach (var item in list)
                {
                    json.Add(item, "0");
                }
            }

            if (!File.Exists("operationalData.json"))
            {
                using (StreamWriter file = File.CreateText("operationalData.json"))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    json.WriteTo(writer);
                }
                return;
            }

            var currentFile = new JObject();
            using (StreamReader r = new StreamReader("operationalData.json"))
            {
                string file = r.ReadToEnd();
                currentFile = JObject.Parse(file);
            }
            currentFile.Add(json.Properties().Where(a => !currentFile.Properties().Select(b => b.Name).Contains(a.Name)));
            using (StreamWriter file = File.CreateText("operationalData.json"))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                currentFile.WriteTo(writer);
            }

            return;
        }
    }
}
