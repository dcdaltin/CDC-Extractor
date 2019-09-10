using Extrator.SQLContext;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extrator.Test.Mock
{
    public class ServiceMocks
    {
        public Mock<IDatabase> GetDatabaseMock()
        {
            Mock<IDatabase> mock = new Mock<IDatabase>();
            mock.Setup(a => a.LastChange(It.IsAny<string>())).ReturnsAsync("asd");
            ICollection<dynamic> data = new List<dynamic>();
            mock.Setup(a => a.GetData(It.IsAny<string>())).ReturnsAsync(data);
            return mock;
        }
    }
}
