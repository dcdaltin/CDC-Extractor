namespace Extrator.Factory
{
    using Extrator.SQLContext;
    using Extrator.SQLContext.MSSQL;
    using Extrator.SQLContext.Oracle;
    using Extrator.SQLContext.Postgres;
    using Microsoft.Extensions.Configuration;
    using System;

    public class DatabaseFactory
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IConfigurationRoot config;

        public DatabaseFactory(IConfigurationRoot config)
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
    }
}
