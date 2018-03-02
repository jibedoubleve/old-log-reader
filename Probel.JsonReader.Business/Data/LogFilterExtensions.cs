using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Probel.JsonReader.Business.Data
{
    public static class LogFilterExtensions
    {
        #region Methods

        public static IEnumerable<LogModel> Filter(this IEnumerable<LogModel> models, int minutes, IFilter filter)
        {
            var levels = new List<string>();
            if (filter.ShowDebug) { levels.Add("DEBUG"); }
            if (filter.ShowInfo) { levels.Add("INFO"); }
            if (filter.ShowWarning) { levels.Add("WARN"); }
            if (filter.ShowError) { levels.Add("ERROR"); }
            if (filter.ShowFatal) { levels.Add("FATAL"); }

            var result = new List<LogModel>();

            var now = DateTime.Now;
            if (minutes == 0)
            {
                result = (from l in models
                          where levels.Contains(l.Level)
                          select l).ToList();
            }
            else
            {
                result = (from l in models
                          where (now - l.Time).TotalMinutes <= minutes
                             && levels.Contains(l.Level)
                          select l).ToList();
            }
            return (filter.IsSortAscending) 
                ? result.OrderBy(e => e.Time) 
                : result.OrderByDescending(e => e.Time);
        }

        public static async Task<IEnumerable<LogModel>> FilterAsync(this IEnumerable<LogModel> models, int minutes, IFilter filter)
        {
            return await Task.Run(() => Filter(models, minutes, filter));
        }

        #endregion Methods
    }
}