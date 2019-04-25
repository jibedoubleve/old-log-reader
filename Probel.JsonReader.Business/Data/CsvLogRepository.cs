using CsvHelper;
using Probel.JsonReader.Business.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace Probel.JsonReader.Business.Data
{
    public class CsvLogRepository : LogRepository
    {
        #region Fields

        private string _sourceString;

        #endregion Fields

        #region Methods

        public override IEnumerable<LogModel> GetLogs()
        {
            var records = new List<LogModel>();

            if (string.IsNullOrEmpty(_sourceString)) { throw new InvalidPathException(); }
            if (!File.Exists(_sourceString)) { throw new InvalidPathException(); }

            for (var i = 0; i < 6; i++)
            {
                try
                {
                    records = RetrieveLogs(_sourceString);
                    LogCache = records;
                    return records;
                }
                catch (FileNotFoundException ex) { Trace.WriteLine($"File '{_sourceString}' busy. Error: {ex.Message}"); }
                catch (IOException ex) { Trace.WriteLine($"File '{_sourceString}' busy. Error: {ex.Message}"); }
                catch (UnauthorizedAccessException ex) { Trace.WriteLine($"File '{_sourceString}' busy. Error: {ex.Message}"); }

                Thread.Sleep(500);
            }

            LogCache = records;
            return records;
        }

        public override IEnumerable<LogModel> Filter(IEnumerable<string> categories, DateTime day, IFilter filter)
        {
            var levels = GetLevels(filter);
            var result = (from l in LogCache
                          where categories.Contains(l.Logger)
                             && levels.Contains(l.Level)
                          select l).ToList();
            return result;
        }

        public override IEnumerable<string> GetCategories()
        {
            if (LogCache == null) { return new List<string>(); }
            var result = (from m in LogCache
                          where !string.IsNullOrEmpty(m.Logger)
                          orderby m.Logger
                          select m.Logger).Distinct()
                                          .ToList();
            return result;
        }

        public override IEnumerable<DateTime> GetDays()
        {
            var result = (from m in LogCache
                          orderby m.Time
                          select m.Time.Date)
                                        .Distinct()
                                        .ToList();

            return result;
        }

        public override string GetSource()
        {
            if (File.Exists(_sourceString)) { return _sourceString; }
            else { return string.Empty; }
        }

        public override void Setup(string sourceString) => _sourceString = sourceString;

        private CsvReader Build(StreamReader reader)
        {
            var csv = new CsvReader(reader);
            csv.Configuration.Quote = '"';
            csv.Configuration.Delimiter = ";";
            csv.Configuration.MissingFieldFound = null;
            return csv;
        }

        private List<LogModel> RetrieveLogs(string path)
        {
            List<LogModel> records;
            using (var stream = File.OpenRead(path))
            using (var reader = new StreamReader(stream))
            {
                var csv = Build(reader);
                records = csv.GetRecords<LogModel>().ToList();
            }

            return records;
        }

        #endregion Methods
    }
}