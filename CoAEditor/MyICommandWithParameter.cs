using System;
using System.Windows.Input;

namespace CoAEditor {
    public class MyICommandWithParameter<T> : ICommand {
        Action<T> _TargetExecuteMethod;
        Func<T, bool> _TargetCanExecuteMethod;

        public MyICommandWithParameter(Action<T> executeMethod) {
            _TargetExecuteMethod = executeMethod;
        }

        public MyICommandWithParameter(Action<T> executeMethod, Func<T, bool> canExecuteMethod) {
            _TargetExecuteMethod = executeMethod;
            _TargetCanExecuteMethod = canExecuteMethod;
        }

        public void OnCanExecuteChanged() {
            CanExecuteChanged(this, EventArgs.Empty);
        }

        bool ICommand.CanExecute(object parameter) {
            if (_TargetCanExecuteMethod != null) {
                T tparm = (T)parameter;
                return _TargetCanExecuteMethod(tparm);
            }

            if (_TargetExecuteMethod != null) {
                return true;
            }

            return false;
        }

        // Should use weak refs if command instance lifetime is longer than lifetime of UI object that get hooked up to command
        public event EventHandler CanExecuteChanged = delegate { };

        void ICommand.Execute(object parameter) {
            if (_TargetExecuteMethod != null)
                _TargetExecuteMethod((T)parameter);
        }
    }
}
