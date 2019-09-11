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

    public class ListenTablesService : IListenTableService
    {
        private readonly IFactory factory;
        private readonly IConfiguration config;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public ListenTablesService(IFactory factory, IConfiguration config)
        {
            this.factory = factory;
            this.config = config;
        }

        private bool HasTableChanges(string table)
        {
            if (!File.Exists("operationalData.json")) new OperationalDataFactory(config).BuildOperationalDataFile();
            JObject fileDataValues;
            using (StreamReader r = new StreamReader("operationalData.json"))
            {
                string file = r.ReadToEnd();
                fileDataValues = JObject.Parse(file);
            }
            string currentValue = factory.GetDatabase().LastChange(table);
            if (!string.Equals(currentValue, fileDataValues.Property(table).Value.ToString()))
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

        public IEnumerable<JObject> GetMessageData(string querySection)
        {
            var data = factory.GetDatabase().GetData(querySection);
            foreach (var item in data)
            {
                var jsonMessage = new JObject();
                jsonMessage.Add("Timestamp", DateTime.Now);
                jsonMessage.Add("Data", JsonConvert.SerializeObject(item));
                yield return jsonMessage;
            }
        }
    }
}
