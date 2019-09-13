namespace Extrator.SQLContext.MSSQL
{
    using Dapper;
    using Microsoft.Extensions.Configuration;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Text;
    using System.Threading.Tasks;

    public class MSSQLContext : IDatabase
    {
        private readonly IConfiguration config;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly string trackingTemplate = @"	
    SELECT 
		max(tlog.[Current LSN])
	FROM 
		sys.objects so
		inner join sys.partitions p on p.object_id=so.object_id
		inner join sys.system_internals_allocation_units AU on p.partition_id=AU.container_id
		inner join(
			select [Current LSN], [transaction ID] tranID,[end time] endTime, AllocUnitId, operation, Context
			from ::fn_dbLog(null, null)
			where (operation in ('LOP_INSERT_ROWS', 'LOP_MODIFY_ROW', 'LOP_DELETE_ROWS'))
			   --and context not in ('LCX_PFS', 'LCX_IAM'))
			--or operation in('LOP_COMMIT_XACT','LOP_BEGIN_XACT')
			)tlog on tlog.AllocUnitId=AU.allocation_unit_id
		inner join 
		(
		   select [Transaction ID] tranID, [End Time] endTime
		   from ::fn_dbLog(null, null)
		   where Operation = 'LOP_COMMIT_XACT' and cast([End Time] as date)>=DateADD(mi, -1500, CURRENT_TIMESTAMP)
	   ) c on tlog.tranID = c.tranID
	WHERE
			so.type='U'
			and object_name(p.object_id) = '[table]'";

        public MSSQLContext(IConfiguration config)
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
                using (var db = new SqlConnection(conString))
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
            using (var db = new SqlConnection(conString))
            {
                foreach (var item in db.Query(query, buffered: false))
                {
                    yield return item;
                }
            }
        }
    }
}
