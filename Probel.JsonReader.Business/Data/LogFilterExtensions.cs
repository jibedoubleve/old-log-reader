using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Probel.JsonReader.Business.Data
{
    public static class LogFilterExtensions
    {
        #region Methods

        public static IEnumerable<LogModel> Filter(this IEnumerable<LogModel> models, int minutes)
        {
            var now = DateTime.Now;
            if (minutes == 0) { return models; }
            else
            {
                return (from l in models
                        where (now - l.Time).TotalMinutes <= minutes
                        select l).ToList();
            }
        }

        public static async Task<IEnumerable<LogModel>> FilterAsync(this IEnumerable<LogModel> models, int minutes)
        {
            return await Task.Run(() => Filter(models, minutes));
        }

        #endregion Methods
    }
}