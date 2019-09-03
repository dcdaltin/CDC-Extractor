namespace Extrator.Factory
{
    using Extrator.SQLContext;
    using Extrator.SQLContext.MSSQL;
    using Extrator.SQLContext.Oracle;
    using Extrator.SQLContext.Postgres;
    using Microsoft.Extensions.Configuration;

    public class DatabaseFactory
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        public IDatabase GetDatabase(IConfigurationRoot config)
        {
            Logger.Debug("Getting database config...");
            string driver;
            try
            {
                driver = config.GetSection("Driver").Value.ToUpperInvariant();
                Logger.Info($"Driver: {driver}");
                switch (driver)
                {
                    case "MSSQL":
                        return new MSSQLContext();
                    case "ORACLE":
                        return new OracleContext();
                    case "POSTGRES":
                        return new PostgresContext();
                    default:
                        {
                            Logger.Error($"Driver not implemented: {driver}");
                            return null;
                        }                        
                }
            }
            catch (System.Exception e)
            {
                Logger.Error(e, "Driver config not found.");
                return null;
            }            
        }
    }
}
