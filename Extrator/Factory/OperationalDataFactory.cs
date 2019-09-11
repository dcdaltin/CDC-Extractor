namespace Extrator.Factory
{
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Linq;

    public class OperationalDataFactory
    {
        private IConfiguration config;

        public OperationalDataFactory(IConfiguration config)
        {
            this.config = config;
        }

        public void BuildOperationalDataFile()
        {
            var sections = config.GetSection("ListenedTables").GetChildren();
            if (!sections.Any()) throw new NullReferenceException("[ListenedTables]");
            var hasSameSections = !config.GetSection("Queries").GetChildren().Where(a => !sections.Select(b => b.Key).Contains(a.Key)).Any();
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
    }
}
