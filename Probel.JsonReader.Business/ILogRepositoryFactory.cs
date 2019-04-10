namespace Probel.JsonReader.Business
{
    public interface ILogRepositoryFactory
    {
        #region Methods

        ILogRepository Get();

        #endregion Methods
    }
}