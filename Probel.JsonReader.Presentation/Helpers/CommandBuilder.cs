using Prism.Commands;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Probel.JsonReader.Presentation.Helpers
{
    public interface ICommandBuilder
    {
        #region Methods

        ICommand BuildAsyncCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod = null);

        ICommand BuildAsyncCommand<T>(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod = null);

        ICommand BuildCommand(Action executeMethod, Func<bool> canExecuteMethod = null);

        ICommand BuildCommand<T>(Action<T> executeMethod, Func<T, bool> canExecuteMethod = null);

        #endregion Methods
    }

    public class DelegateCommandBuilder : ICommandBuilder
    {
        #region Methods

        public ICommand BuildAsyncCommand(Func<Task> executeMethod, Func<bool> canExecuteMethod = null) => new DelegateCommand(async () => await executeMethod(), canExecuteMethod ?? (() => true));

        public ICommand BuildAsyncCommand<T>(Func<T, Task> executeMethod, Func<T, bool> canExecuteMethod = null)
        {
            if (TypeHelper.IsNullable<T>())
            {
                return new DelegateCommand<T>(async t => await executeMethod(t), canExecuteMethod ?? (t => true));
            }
            else
            {
                throw new InvalidCastException(
                    $"To build an ICommand, the generic type should be nullable. The provided type '{typeof(T)}' is not nullable");
            }
        }

        public ICommand BuildCommand(Action executeMethod, Func<bool> canExecuteMethod = null) => new DelegateCommand(executeMethod, canExecuteMethod ?? (() => true));

        public ICommand BuildCommand<T>(Action<T> executeMethod, Func<T, bool> canExecuteMethod = null)
        {
            if (TypeHelper.IsNullable<T>())
            {
                return new DelegateCommand<T>(executeMethod, canExecuteMethod ?? (t => true));
            }
            else
            {
                throw new NotSupportedException(
                    $"To build an ICommand, the generic type should be nullable. The provided type '{typeof(T)}' is not nullable");
            }
        }

        #endregion Methods
    }

    internal class TypeHelper
    {
        #region Methods

        public static bool IsNullable<T>()
        {
            var type = typeof(T);
            if (!type.IsValueType) { return true; /*ref-type*/ }
            if (Nullable.GetUnderlyingType(type) != null) { return true; /* Nullable<T>*/ }
            return false; // value-type
        }

        public bool IsNullable<T>(T obj) => (obj == null) ? true /* obvious */ : IsNullable<T>();

        #endregion Methods
    }
}
