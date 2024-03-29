﻿namespace Extrator.SQLContext.Postgres
{
    using Microsoft.Extensions.Configuration;
    using Npgsql;
    using System;
    using Dapper;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class PostgresContext : IDatabase
    {
        private readonly IConfiguration config;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static string trackingTemplate = "select max(xmin::text::bigint)::varchar from [table]";

        public PostgresContext(IConfiguration config)
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
                using (var db = new NpgsqlConnection(conString))
                {
                    return db.QuerySingleOrDefault<string>(sql);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, $"[ListenedTables]: {tableName}");
                throw;
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
            using (var db = new NpgsqlConnection(conString))
            {
                foreach (var item in db.Query(query))
                {
                    yield return item;
                }
            }
        }
    }
}
