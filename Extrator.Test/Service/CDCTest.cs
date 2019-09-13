using Extrator.Factory;
using Extrator.Service;
using Extrator.Test.Mock;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Extrator.Test.Service
{
    public class CDCTest
    {
        [Fact]
        void Check()
        {
            var mock = new ServicesMock();
            var service = new CDCService(mock.GetFactoryMock().Object,mock.GetConfigMock().Object);
            service.Run();
        }
    }
}
