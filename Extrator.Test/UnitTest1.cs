namespace Extrator.Test
{
    using Xunit;
    using Extrator.Factory;
    using System;
    using System.IO;

    public class UnitTest1
    {
        [Fact]
        public void VerifyConfigFactory()
        {
            var config = new ConfigFactory().BuildConfig();
            var children = config.GetChildren();
            Assert.NotNull(children);
        }

        [Fact]
        public void VerifyLoggerFactory()
        {
            new ConfigFactory().BuildLogConfig();
            NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
            Logger.Debug("Teste");
            Assert.NotNull(Logger);
        }

        [Fact]
        public void VerifyOperationDataFileFactory()
        {
            var config = new ConfigFactory().BuildConfig();
            new ConfigFactory().BuildOperationalDataFile(config);
            var hasFile = File.Exists("operationalData.json");
            var isEmpty = File.ReadAllLines("operationalData.json").Length == 0 ? true : false;
            Assert.True(hasFile);
            Assert.False(isEmpty);
        }
    }
}
