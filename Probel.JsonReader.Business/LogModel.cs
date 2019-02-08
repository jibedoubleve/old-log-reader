using System;

namespace Probel.JsonReader.Business
{
    public class LogModel
    {
        #region Properties

        public string Exception { get; set; }
        public string Level { get; set; }
        public string Logger { get; set; }
        public string Message { get; set; }
        public int ThreadId { get; set; }
        public DateTime Time { get; set; }

        #endregion Properties
    }
}