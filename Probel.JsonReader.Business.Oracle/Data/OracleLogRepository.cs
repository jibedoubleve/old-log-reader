using Dapper;
using Oracle.ManagedDataAccess.Client;
using Probel.JsonReader.Business.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Probel.JsonReader.Business.Oracle.Data
{
    public class OracleLogRepository : LogRepository
    {
        #region Fields

        private string _connectionString;

        #endregion Fields

        #region Methods

        public override IEnumerable<LogModel> Filter(IEnumerable<string> categories, DateTime day, IFilter filter)
        {
            var sql = @"
                select loglevel  as ""Level""
                     , logsource as Logger
                     , message   as Message
                     , logtime   as Time
                from app_log
                where trunc(logtime) = trunc(:day)
                order by logtime desc";


            using (var c = new OracleConnection(_connectionString))
            {
                var result = c.Query<LogModel>(sql, new { day });

                if ((categories?.Count() ?? 0) > 0)
                {
                    var levels = GetLevels(filter);
                    result = (from l in result
                              where categories.Contains(l.Logger)
                                 && levels.Contains(l.Level)
                              select l).ToList();
                }
                LogCache = result;
                return result;
            }
        }

        public override IEnumerable<string> GetCategories()
        {
            var sql = @"
                select distinct logsource
                from app_log
                order by logsource";
            using (var c = new OracleConnection(_connectionString))
            {
                var result = c.Query<string>(sql);
                return result;
            }
        }

        public override IEnumerable<DateTime> GetDays()
        {
            var sql = @"
                    select distinct trunc(logtime)
                    from app_log
                    where trunc(logtime) > trunc(sysdate - 60)
                    order by trunc(logtime) desc";
            using (var c = new OracleConnection(_connectionString))
            {
                var result = c.Query<DateTime>(sql);
                return result;
            }
        }

        public override IEnumerable<LogModel> GetLogs()
        {
            CacheLogs();
            return LogCache;
        }

        public override string GetSource() => _connectionString;

        public override void Setup(string connectionString) => _connectionString = connectionString;

        private void CacheLogs()
        {
            var sql = @"
                select loglevel  as ""Level""
                     , logsource as Logger
                     , message   as Message
                     , logtime   as Time
                from app_log
                where trunc(logtime) >= trunc(sysdate)";

            using (var c = new OracleConnection(_connectionString))
            {
                var result = c.Query<LogModel>(sql);
                LogCache = result;
            }
        }

        #endregion Methods
    }
}