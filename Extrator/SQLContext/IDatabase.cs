namespace Extrator.SQLContext
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDatabase
    {
        Task<string> LastChange(string tableName);
        Task<IEnumerable<dynamic>> GetData(string querySectionField);
    }
}
