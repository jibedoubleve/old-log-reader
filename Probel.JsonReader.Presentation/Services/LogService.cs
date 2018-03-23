using NLog;
using System;

namespace Probel.JsonReader.Presentation.Services
{
    public interface ILogService
    {
        #region Methods

        void Debug(string message);

        void Error(string message, Exception ex = null);

        void Fatal(string message, Exception ex = null);

        void Info(string message);

        void Trace(string message);

        void Warn(string message, Exception ex = null);

        #endregion Methods
    }

    public class NLogService : ILogService
    {
        #region Fields

        private Logger _logger = LogManager.GetLogger("");

        #endregion Fields

        #region Methods

        public void Debug(string message) => _logger.Debug(message);

        public void Error(string message, Exception ex = null)
        {
            if (ex == null) { _logger.Error(message); }
            else { _logger.Error(ex, message); }
        }

        public void Fatal(string message, Exception ex = null)
        {
            if (ex == null) { _logger.Fatal(message); }
            else { _logger.Fatal(ex, message); }
        }

        public void Info(string message) => _logger.Info(message);

        public void Trace(string message) => _logger.Trace(message);

        public void Warn(string message, Exception ex = null)
        {
            if (ex == null) { _logger.Warn(message); }
            else { _logger.Warn(ex, message); }
        }

        #endregion Methods
    }
}