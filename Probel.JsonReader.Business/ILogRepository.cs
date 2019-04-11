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

        IEnumerable<LogModel> Filter(IEnumerable<string> categories, DateTime day, IFilter filters);

        IEnumerable<string> GetCategories();

        IEnumerable<DateTime> GetDays();

        IEnumerable<LogModel> GetLogs();

        string GetSource();

        void Setup(string connectionString);

        #endregion Methods
    }
}