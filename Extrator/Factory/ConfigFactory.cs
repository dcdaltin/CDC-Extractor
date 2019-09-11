namespace Extrator.Factory
{
    using Microsoft.Extensions.Configuration;
    using NLog;
    using System.IO;

    public class ConfigFactory
    {
        public IConfigurationRoot BuildConfig()
        {
            var configbuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("dbsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot appconfiguration = configbuilder.Build();

            return appconfiguration;
        }

        public void BuildLogConfig()
        {
            var config = new NLog.Config.LoggingConfiguration();

            // Targets where to log to: File and Console
            var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "logs.txt" };
            var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            // Apply config
            LogManager.Configuration = config;
        }
    }
}
