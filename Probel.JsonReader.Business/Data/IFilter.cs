namespace Probel.JsonReader.Business.Data
{
    public interface IFilter
    {
        #region Properties

        bool ShowTrace { get; }
        bool ShowDebug { get; }
        bool ShowError { get; }
        bool ShowFatal { get; }
        bool ShowInfo { get; }
        bool ShowWarning { get; }

        bool IsSortAscending { get; }
        #endregion Properties
    }
}