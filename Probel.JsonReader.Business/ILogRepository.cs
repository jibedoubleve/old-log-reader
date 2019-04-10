using Probel.JsonReader.Business.Data;
using System;
using System.Collections.Generic;

namespace Probel.JsonReader.Business
{
    public interface ILogRepository
    {
        #region Methods

        IEnumerable<LogModel> Filter(decimal minutes, IFilter filter);

        IEnumerable<LogModel> Filter(IEnumerable<string> categories, IFilter filter);

        IEnumerable<LogModel> Filter(IEnumerable<string> categories, decimal minutes, IFilter filter);

        IEnumerable<LogModel> GetAllLogs();

        IEnumerable<string> GetCategories();

        IEnumerable<DateTime> GetDays();

        string GetSource();

        void Setup(string connectionString);

        #endregion Methods
    }
}