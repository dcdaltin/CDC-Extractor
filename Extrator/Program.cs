namespace Extrator
{
    using Extrator.Factory;
    using Extrator.Job;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using System.IO;
    using System.Threading.Tasks;
    using NLog;
    using NLog.Extensions.Logging;
    using Extrator.Service;

    class Program
    {
        public static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureLogging(factory =>
                {
                    factory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });
                    var Nconfig = new NLog.Config.LoggingConfiguration();

                    // Targets where to log to: File and Console
                    var logfile = new NLog.Targets.FileTarget("logfile") { FileName = "logs.txt" };
                    var logconsole = new NLog.Targets.ConsoleTarget("logconsole");

                    // Rules for mapping loggers to targets            
                    Nconfig.AddRule(LogLevel.Info, LogLevel.Fatal, logconsole);
                    Nconfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
                    LogManager.Configuration = Nconfig;
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory());
                    config.AddJsonFile("dbsettings.json", optional: false, reloadOnChange: false);
                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IFactory, Factory.Factory>();
                    services.AddSingleton<IListenTableService, ListenTablesService>();

                    services.AddSingleton<TimedHostedService>();

                    services.AddLogging();

                    #region snippet1
                    services.AddHostedService<TimedHostedService>();
                    #endregion
                })
                .UseConsoleLifetime()
                .Build();

            using (host)
            {
                // Start the host
                await host.StartAsync();

                // Wait for the host to shutdown
                await host.WaitForShutdownAsync();
            }
        }
    }
}
