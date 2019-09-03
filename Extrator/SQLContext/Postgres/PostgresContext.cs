using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extrator.SQLContext.Postgres
{
    public class PostgresContext : IDatabase
    {
        private readonly IConfigurationRoot config;

        public PostgresContext(IConfigurationRoot config)
        {
            this.config = config;
        }


    }
}
