using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Extrator.SQLContext.Oracle
{
    public class OracleContext : IDatabase
    {
        public async Task<ICollection<Event>> GetEvents()
        {
            using (OracleConnection connection = new OracleConnection(_conString))
            {
                var result = await connection.QueryAsync<Event>(_queryString, commandTimeout: 0);
                return result.AsList();
            }
        }
    }
}
