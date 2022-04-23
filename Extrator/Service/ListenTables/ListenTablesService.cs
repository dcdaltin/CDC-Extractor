namespace Extrator.Service
{
    using Extrator.Factory;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using System;
    using System.Threading.Tasks;

    public class ListenTablesService : IListenTable
    {
        private readonly IFactory factory;
        private readonly IConfiguration config;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private IDictionary<string, string> changes;

        public ListenTablesService(IFactory factory, IConfiguration config)
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
            var sections = config.GetSection("ALL").GetSection("ListenedTables").GetChildren();
            ICollection<string> result = new List<string>();
            bool test = false;
            foreach (var section in sections)
            {
                foreach (var table in section.GetChildren().Select(a => a.Value))
                {
                    var changeTest = HasTableChanges(table);
                    test = test || changeTest;
                }
                if (test) result.Add(section.Key);
                test = false;
            }
            return result.Distinct().ToList();
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

        internal IEnumerable<string> GetMessageData(string querySection)
        {
            var data = factory.GetDatabase().GetData(querySection, new Dictionary<string, string>());
            foreach (var item in data)
            {
                yield return JsonConvert.SerializeObject(item);
            }
        }

        internal void BuildOperationalDataFile()
        {
            var sections = config.GetSection("ALL").GetSection("ListenedTables").GetChildren();
            if (!sections.Any()) throw new NullReferenceException("[ListenedTables]");
            var hasSameSections = !config.GetSection("ALL").GetSection("Queries").GetChildren().Where(a => !sections.Select(b => b.Key).Contains(a.Key)).Any();
            if (!hasSameSections) throw new InvalidDataException("[ListenedTables] and [Queries] fields do not match");
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

        public void Run()
        {
            Logger.Info("Checking for changes...");
            BuildOperationalDataFile();
            var sectionChanges = CheckSectionChanges();
            foreach (var section in sectionChanges)
            {
                Logger.Info($"Sending messages for section {section}");
                var data = GetMessageData(section);
                foreach (var item in data)
                {
                    factory.GetMessagingService().SendMessage(section, item);
                }
                Logger.Info($"Messages sent!");
            }

            RefreshOperationalDataFile();
        }
    }
}
