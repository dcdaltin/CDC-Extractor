namespace Extrator.SQLContext.Oracle
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Dapper;
    using global::Oracle.ManagedDataAccess.Client;
    using Microsoft.Extensions.Configuration;

    public class OracleContext : IDatabase
    {
        private readonly IConfiguration config;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly string trackingTemplate = "select TO_CHAR(max(ora_rowscn)) from [table]";

        public OracleContext(IConfiguration config)
        {
            this.config = config;
        }

        public string LastChange(string tableName)
        {
            Logger.Debug($"Loking for changes on table {tableName}");
            var sql = new StringBuilder(trackingTemplate).Replace("[table]", tableName).ToString();
            Logger.Debug("Getting connection string...");
            string conString = config.GetSection("ALL").GetSection("ConnectionString").Value;
            if (string.IsNullOrEmpty(conString)) throw new NullReferenceException("[ConnectionString]");
            try
            {
                Logger.Debug($"Connecting e running query: {sql}");
                using (var db = new OracleConnection(conString))
                {
                    return db.QuerySingleOrDefault<string>(sql);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"[ListenedTables]: {tableName}");
                throw e;
            }
        }

        public IEnumerable<dynamic> GetData(string querySectionField, IDictionary<string, string> param)
        {
            Logger.Debug($"Getting query from field {querySectionField}");
            var conString = config.GetSection("ALL").GetSection("ConnectionString").Value;
            if (string.IsNullOrEmpty(conString)) throw new NullReferenceException("[ConnectionString]");
            var query = config.GetSection("ALL").GetSection("Queries")[querySectionField];
            if (string.IsNullOrEmpty(conString)) throw new NullReferenceException("[Queries]");
            Logger.Debug($"Connecting e running query: {query}");
            using (var db = new OracleConnection(conString))
            {
                foreach (var item in db.Query(query))
                {
                    yield return item;
                }
            }
        }
    }
}
