using Probel.JsonReader.Business.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Probel.JsonReader.Business
{
    public abstract class LogRepository : ILogRepository
    {
        #region Fields

        private IEnumerable<LogModel> _logCache;

        #endregion Fields

        #region Properties

        protected IEnumerable<LogModel> LogCache
        {
            get
            {
                if (_logCache == null) { _logCache = new List<LogModel>(); }
                return _logCache;
            }
            set => _logCache = value;
        }

        #endregion Properties

        #region Methods

        public IEnumerable<LogModel> Filter(decimal minutes, IFilter filter)
        {
            if (LogCache == null) { LogCache = GetLogs(); }

            var now = DateTime.Now;
            var levels = GetLevels(filter);
            var result = new List<LogModel>();

            if (minutes == 0)
            {
                result = (from l in LogCache
                          where levels.Contains(l.Level)
                          select l).ToList();
            }
            else
            {
                var seconds = (long)(minutes * 60);
                result = (from l in LogCache
                          where (now - l.Time).TotalSeconds <= seconds
                             && levels.Contains(l.Level)
                          select l).ToList();
            }
            return (filter.IsSortAscending)
                ? result.OrderBy(e => e.Time)
                : result.OrderByDescending(e => e.Time);
        }

        public IEnumerable<LogModel> Filter(IEnumerable<string> categories, IFilter filter)
        {
            IEnumerable<LogModel> result;
            var levels = GetLevels(filter);

            if (LogCache == null)
            {
                LogCache = GetLogs();
                result = LogCache;
            }
            else if (categories.Count() == 0) { result = LogCache.ToList(); }
            else
            {
                result = (from m in LogCache
                          where categories.Contains(m.Logger)
                             && levels.Contains(m.Level)
                          select m)
                             .ToList();
            }
            return (filter.IsSortAscending)
                ? result.OrderBy(e => e.Time)
                : result.OrderByDescending(e => e.Time);
        }

        public IEnumerable<LogModel> Filter(IEnumerable<string> categories, decimal minutes, IFilter filter)
        {
            IEnumerable<LogModel> result;

            var now = DateTime.Now;
            var levels = GetLevels(filter);
            long seconds = (minutes == 0) ? long.MaxValue : (long)(minutes * 60);

            if (LogCache == null)
            {
                LogCache = GetLogs();
                result = LogCache;
            }
            else if ((categories?.Count() ?? 0) == 0) { result = LogCache; }
            else
            {
                result = (from l in LogCache
                          where categories.Contains(l.Logger)
                             && levels.Contains(l.Level)
                             && (now - l.Time).TotalSeconds <= seconds
                          select l)
                              .ToList();
            }

            return (filter.IsSortAscending)
                ? result.OrderBy(e => e.Time)
                : result.OrderByDescending(e => e.Time);
        }

        public abstract IEnumerable<LogModel> Filter(IEnumerable<string> categories, DateTime day, IFilter filters);

        public abstract IEnumerable<string> GetCategories();

        public abstract IEnumerable<DateTime> GetDays();

        public abstract IEnumerable<LogModel> GetLogs();

        public abstract string GetSource();

        public abstract void Setup(string connectionString);

        protected static IEnumerable<string> GetLevels(IFilter filter)
        {
            var levels = new List<string>();
            if (filter.ShowTrace) { levels.Add("TRACE"); }
            if (filter.ShowDebug) { levels.Add("DEBUG"); }
            if (filter.ShowInfo) { levels.Add("INFO"); }
            if (filter.ShowWarning) { levels.Add("WARN"); }
            if (filter.ShowError) { levels.Add("ERROR"); }
            if (filter.ShowFatal) { levels.Add("FATAL"); }

            return levels;
        }

        #endregion Methods
    }
}