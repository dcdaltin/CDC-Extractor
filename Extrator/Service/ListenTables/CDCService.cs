namespace Extrator.Service
{
    using Extrator.Factory;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class CDCService : IListenTable
    {
        private readonly IFactory factory;
        private readonly IConfiguration config;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private IDictionary<string, string> changes;

        public CDCService(IFactory factory, IConfiguration config)
        {
            this.factory = factory;
            this.config = config;
            changes = new Dictionary<string, string>();
        }

        internal bool HasTableChanges(string table)
        {
            JObject fileDataValues;
            using (StreamReader r = new StreamReader("operationalData.json"))
            {
                string file = r.ReadToEnd();
                fileDataValues = JObject.Parse(file);
            }
            string currentValue = factory.GetDatabase().LastChange(table);
            if (string.Equals(currentValue, fileDataValues.Property(table).Value.ToString())) return false;
            changes.Add(table, currentValue);
            return true;
        }

        internal ICollection<string> CheckSectionChanges()
        {
            var entities = config.GetSection("CDC").GetSection("Entities").GetChildren();
            ICollection<string> result = new List<string>();
            bool test = false;
            foreach (var entity in entities)
            {
                foreach (var table in entity.GetSection("Listener").GetChildren().Select(a => a["Table"]))
                {
                    var changeTest = HasTableChanges(table);
                    test = test || changeTest;
                }
                if (test) result.Add(entity.Key);
                test = false;
            }
            return result.Distinct().ToList();
        }

        internal IEnumerable<string> GetMessageData(string querySection, dynamic param)
        {
            var data = factory.GetDatabase().GetData(querySection, param);
            foreach (var item in data)
            {
                yield return JsonConvert.SerializeObject(item);
            }
        }

        internal void BuildOperationalDataFile()
        {
            var sections = config.GetSection("CDC").GetSection("Entities").GetChildren();
            if (!sections.Any()) throw new NullReferenceException("[Entities]");
            var json = new JObject();
            var tables = sections.Select(a => a.GetSection("Listener").GetChildren().Select(b => b["Table"]));
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

        internal void RefreshOperationalDataFile()
        {
            JObject fileDataValues;
            using (StreamReader r = new StreamReader("operationalData.json"))
            {
                string file = r.ReadToEnd();
                fileDataValues = JObject.Parse(file);
            }

            using (StreamWriter file = File.CreateText("operationalData.json"))
            using (JsonTextWriter writer = new JsonTextWriter(file))
            {
                foreach (var item in changes)
                {
                    fileDataValues.Property(item.Key).Value = item.Value;
                }
                fileDataValues.WriteTo(writer);
            }

            changes = new Dictionary<string, string>();
        }

        public void Run()
        {
            Logger.Info("Checking for changes...");
            BuildOperationalDataFile();
            var sectionChanges = CheckSectionChanges();
            var currentFile = new JObject();
            using (StreamReader r = new StreamReader("operationalData.json"))
            {
                string file = r.ReadToEnd();
                currentFile = JObject.Parse(file);
            }
            foreach (var section in sectionChanges)
            {
                var param = new Dictionary<string, string>();
                var configSection = config.GetSection("CDC").GetSection("Entities").GetChildren().Where(a => a.Key == section).SingleOrDefault().GetSection("Listener").GetChildren();
                foreach (var item in configSection)
                {
                    param.Add(
                        item["QueryParameter"],
                        string.IsNullOrEmpty(currentFile.Property(item["Table"]).Value.ToString()) ? "0" : currentFile.Property(item["Table"]).Value.ToString()
                        );
                }
                Logger.Info($"Sending messages for section {section}");
                var data = GetMessageData(section, param);
                foreach (var item in data)
                {
                    factory.GetMessagingService().SendMessage(section, item);
                }
                Logger.Info($"Messages sent!");
                RefreshOperationalDataFile();
            }
        }
    }
}
