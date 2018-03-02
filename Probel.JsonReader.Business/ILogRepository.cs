using System.Collections.Generic;

namespace Probel.JsonReader.Business
{
    public interface ILogRepository
    {
        #region Methods

        IEnumerable<LogModel> GetAllLogs(string path);
        
        #endregion Methods
    }
}