using System.Collections.Generic;
using System.Text;

namespace Probel.JsonReader.Business
{
    public interface ILogRepository
    {
        #region Methods

        IEnumerable<LogModel> GetAllLogs(string path, Encoding encoding = null);

        #endregion Methods
    }
}