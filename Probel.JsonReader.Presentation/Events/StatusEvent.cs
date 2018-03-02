using Prism.Events;
using Probel.JsonReader.Presentation.Constants;

namespace Probel.JsonReader.Presentation.Events
{
    public class StatusEvent : PubSubEvent<StatusEventContext> { }

    public class StatusEventContext
    {
        #region Constructors

        public StatusEventContext(StatusLevel level, string message)
        {
            Level = level;
            Message = message;
        }

        #endregion Constructors

        #region Properties

        public StatusLevel Level { get; private set; }

        public string Message { get; private set; }

        #endregion Properties
    }
}