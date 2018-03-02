using System.Windows.Input;

namespace Probel.JsonReader.Presentation.Helpers
{
    public static class CommandExtensions
    {
        #region Methods

        public static void TryExecute(this ICommand command, object parameter = null)
        {
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }

        public static void TryExecute<T>(this ICommand command, T parameter)
        {
            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }

        #endregion Methods
    }
}