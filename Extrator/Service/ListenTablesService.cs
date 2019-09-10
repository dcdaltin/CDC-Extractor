namespace Extrator.Service
{
    using Extrator.Factory;
    using Extrator.SQLContext;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using System;

    public class ListenTablesService
    {
        private readonly IConfigurationRoot config;
        private readonly IDatabase database;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public ListenTablesService(IConfigurationRoot config, IDatabase database)
        {
            this.config = config;
            this.database = database;
        }

        public bool HasTableChanges(string table)
        {
            if (!File.Exists("operationalData.json")) new ConfigFactory().BuildOperationalDataFile(config);
            JObject fileDataValues;
            using (StreamReader r = new StreamReader("operationalData.json"))
            {
                string file = r.ReadToEnd();
                fileDataValues = JObject.Parse(file);
            }
            var currentValue = database.LastChange(table).Result;
            if(!string.Equals(currentValue, fileDataValues.Property(table).Value.ToString()))
            {
                fileDataValues.Property(table).Value = currentValue;
                using (StreamWriter file = File.CreateText("operationalData.json"))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    fileDataValues.WriteTo(writer);
                }
                return true;
            }

            return false;
        }

        public ICollection<string> CheckSectionChanges()
        {
            var sections = config.GetSection("ListenedTables").GetChildren();
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

        public JObject GetMessageData(string querySection)
        {
            var data = database.GetData(querySection).Result;
            var jsonMessage = new JObject();
            jsonMessage.Add("Timestamp", DateTime.Now);
            jsonMessage.Add(data);
            return jsonMessage;
        }
    }
}
