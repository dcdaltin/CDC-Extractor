namespace Extrator.SQLContext
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDatabase
    {
        string LastChange(string tableName);
        IEnumerable<dynamic> GetData(string querySectionField, IDictionary<string, string> param);
    }
}
