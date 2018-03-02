using Prism.Events;
using Probel.JsonReader.Presentation.Constants;
using Probel.JsonReader.Presentation.Events;

namespace Probel.JsonReader.Presentation.Services
{
    public interface IStatusService
    {
        #region Methods

        void Write(StatusLevel level, string message);

        #endregion Methods
    }

    public class StatusService : IStatusService
    {
        #region Fields

        private readonly IEventAggregator EventAggregator;

        #endregion Fields

        #region Constructors

        public StatusService(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;
        }

        #endregion Constructors

        #region Methods

        public void Write(StatusLevel level, string message) => EventAggregator.GetEvent<StatusEvent>().Publish(new StatusEventContext(level, message));

        #endregion Methods
    }
}