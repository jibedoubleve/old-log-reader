using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;

namespace Probel.JsonReader.Business.Oracle.Data
{
    public class OracleLogRepository : LogRepository
    {
        #region Fields

        private string _connectionString;

        #endregion Fields

        #region Methods

        public override IEnumerable<LogModel> GetAllLogs()
        {
            CacheLogs();
            return LogCache;
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
                    order by trunc(logtime)";
            using (var c = new OracleConnection(_connectionString))
            {
                var result = c.Query<DateTime>(sql);
                return result;
            }
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
                where trunc(logtime) >= trunc(sysdate - 1)";

            using (var c = new OracleConnection(_connectionString))
            {
                var result = c.Query<LogModel>(sql);
                LogCache = result;
            }
        }

        #endregion Methods
    }
}