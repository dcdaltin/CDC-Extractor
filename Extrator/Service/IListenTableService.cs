using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extrator.Service
{
    interface IListenTableService
    {
        ICollection<string> CheckSectionChanges();
        IEnumerable<JObject> GetMessageData(string querySection);
    }
}
