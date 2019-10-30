using CsvHelper;
using CsvHelper.Configuration;
using Probel.JsonReader.Business.Exception;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Probel.JsonReader.Business.Data
{
    public class LogRepository : ILogRepository
    {
        #region Methods
        private Encoding _encoding;
        public IEnumerable<LogModel> GetAllLogs(string path, Encoding encoding = null)
        {
            _encoding = encoding ?? Encoding.UTF8;
            var records = new List<LogModel>();

            if (string.IsNullOrEmpty(path)) { throw new InvalidPathException(); }
            if (!File.Exists(path)) { throw new InvalidPathException(); }

            for (var i = 0; i < 6; i++)
            {
                try
                {
                    records = RetrieveLogs(path);
                    return records;
                }
                catch (FileNotFoundException ex) { Trace.WriteLine($"File '{path}' busy. Error: {ex.Message}"); }
                catch (IOException ex) { Trace.WriteLine($"File '{path}' busy. Error: {ex.Message}"); }
                catch (UnauthorizedAccessException ex) { Trace.WriteLine($"File '{path}' busy. Error: {ex.Message}"); }

                Thread.Sleep(500);
            }
            return records;
        }

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
            using (var reader = new StreamReader(stream, _encoding))
            using (var csv = Build(reader))
            {
                records = csv.GetRecords<LogModel>().ToList();
            }

            return records;
        }

        #endregion Methods
    }
}