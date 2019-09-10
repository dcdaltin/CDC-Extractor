namespace Extrator.Test
{
    using Extrator.Factory;
    using Extrator.Service;
    using Extrator.Test.Mock;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Xunit;

    public class ListenTablesServiceTest
    {        
        [Fact]
        public void CheckSectionChangesTest()
        {
            var config = new ConfigFactory().BuildConfig();
            var database = new ServiceMocks().GetDatabaseMock().Object;
            var service = new ListenTablesService(config, database);
            var result = service.CheckSectionChanges();
            Assert.InRange(result.Count,0,config.GetSection("ListenedTables").GetChildren().Count());
        }
    }
}
