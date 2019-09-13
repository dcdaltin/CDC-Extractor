using Extrator.Factory;
using Extrator.SQLContext;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extrator.Test.Mock
{
    public class ServicesMock
    {
        public Mock<IDatabase> GetDatabaseMock()
        {
            Mock<IDatabase> mock = new Mock<IDatabase>();
            return mock;
        }

        public Mock<IConfiguration> GetConfigMock()
        {
            Mock<IConfiguration> mock = new Mock<IConfiguration>();
            return mock;
        }

        public Mock<IFactory> GetFactoryMock()
        {
            Mock<IFactory> mock = new Mock<IFactory>();
            return mock;
        }
    }
}
