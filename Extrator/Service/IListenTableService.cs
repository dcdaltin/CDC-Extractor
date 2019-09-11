namespace Extrator.Service
{
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Text;

    interface IListenTableService
    {
        ICollection<string> CheckSectionChanges();
        IEnumerable<string> GetMessageData(string querySection);
    }
}
