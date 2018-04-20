using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Probel.JsonReader.Business.Data
{
    public static class LogFilterExtensions
    {
        #region Methods

        public static IEnumerable<string> GetCategories(this IEnumerable<LogModel> models)
        {
            if (models == null) { return new List<string>(); }
            var result = (from m in models select m.Logger)
                .OrderBy(e => e)
                .Distinct()
                .ToList();
            return result;

        }

        public static IEnumerable<LogModel> Filter(this IEnumerable<LogModel> models, decimal minutes, IFilter filter)
        {
            var levels = new List<string>();
            if (filter.ShowTrace) { levels.Add("TRACE"); }
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
                var seconds = (long)(minutes * 60);
                result = (from l in models
                          where (now - l.Time).TotalSeconds <= seconds
                             && levels.Contains(l.Level)
                          select l).ToList();
            }
            return (filter.IsSortAscending)
                ? result.OrderBy(e => e.Time)
                : result.OrderByDescending(e => e.Time);
        }

        public static IEnumerable<LogModel> Filter(this IEnumerable<LogModel> models, IEnumerable<string> categories, IFilter filter)
        {
            var result = (from m in models
                          where categories.Contains(m.Logger)
                          select m)
                          .ToList();

            return (filter.IsSortAscending)
                ? result.OrderBy(e => e.Time)
                : result.OrderByDescending(e => e.Time);
        }

        #endregion Methods
    }
}