namespace Extrator.SQLContext.Postgres
{
    using Dapper;
    using Microsoft.Extensions.Configuration;
    using Npgsql;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class PostgresCDC : IDatabase
    {
        private readonly IConfiguration config;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly string trackingTemplate = "select TO_CHAR(max(ora_rowscn)) from [table]";

        public PostgresCDC(IConfiguration config)
        {
            this.config = config;
        }

        public IEnumerable<dynamic> GetData(string querySectionField, IDictionary<string, string> param)
        {
            Logger.Debug($"Getting query from field {querySectionField}");
            var conString = config.GetSection("CDC").GetSection("ConnectionString").Value;
            if (string.IsNullOrEmpty(conString)) throw new NullReferenceException("[ConnectionString]");
            var query = config.GetSection("CDC").GetSection("Entities").GetSection(querySectionField)["Query"];
            if (string.IsNullOrEmpty(conString)) throw new NullReferenceException("[Query]");
            Logger.Debug($"Connecting e running query: {query}");
            var parameter = new DynamicParameters();
            foreach (var item in param)
            {
                parameter.Add(item.Key, item.Value, System.Data.DbType.String, System.Data.ParameterDirection.Input);
            }
            using (var db = new NpgsqlConnection(conString))
            {
                foreach (var item in db.Query(query, parameter))
                {
                    yield return item;
                }
            }
        }

        public string LastChange(string tableName)
        {
            Logger.Debug($"Loking for changes on table {tableName}");
            var sql = new StringBuilder(trackingTemplate).Replace("[table]", tableName).ToString();
            Logger.Debug("Getting connection string...");
            string conString = config.GetSection("CDC").GetSection("ConnectionString").Value;
            if (string.IsNullOrEmpty(conString)) throw new NullReferenceException("[ConnectionString]");
            try
            {
                Logger.Debug($"Connecting e running query: {sql}");
                using (var db = new NpgsqlConnection(conString))
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
    }
}
