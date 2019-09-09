namespace Extrator.Factory
{
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using NLog;
    using System;
    using System.IO;
    using System.Linq;

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

        public void BuildOperationalDataFile(IConfigurationRoot config)
        {
            var sections = config.GetSection("ListenedTables").GetChildren();
            if (!sections.Any()) throw new NullReferenceException("[ListenedTables]");
            var json = new JObject();
            var tables = sections.Select(a => a.GetChildren().Select(b => b.Value));
            foreach (var list in tables)
            {
                foreach (var item in list)
                {
                    json.Add(item, "0");
                }
            }

            if (!File.Exists("operationalData.json"))
            {
                using (StreamWriter file = File.CreateText("operationalData.json"))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    json.WriteTo(writer);
                }
                return;
            }

            var currentFile = new JObject();
            using (StreamReader r = new StreamReader("operationalData.json"))
            {
                string file = r.ReadToEnd();
                currentFile = JObject.Parse(file);
            }
            currentFile.Add(json.Properties().Where(a => !currentFile.Properties().Select(b => b.Name).Contains(a.Name)));
            using (StreamWriter file = File.CreateText("operationalData.json"))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                currentFile.WriteTo(writer);
            }

            return;
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
