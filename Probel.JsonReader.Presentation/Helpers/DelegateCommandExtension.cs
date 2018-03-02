using Prism.Commands;
using System;
using System.Windows.Input;

namespace Probel.JsonReader.Presentation.Helpers
{
    public static class DelegateCommandExtension
    {
        #region Methods

        public static void RaiseCanExecuteChanged(this ICommand command)
        {
            if (command is DelegateCommandBase d)
            {
                d.RaiseCanExecuteChanged();
            }
            else if (command == null) { throw new ArgumentNullException(nameof(command), "The command is null. Did you configured this command in the ctor of the ViewModel?"); }
            else { throw new NotSupportedException($"The command is not of type '{typeof(DelegateCommand)}'"); }
        }

        public static bool TryExecute(this ICommand command, object parameter = null)
        {
            var canExecute = command.CanExecute(parameter);
            if (canExecute)
            {
                command.Execute(parameter);
            }
            return canExecute;
        }

        #endregion Methods
    }
}