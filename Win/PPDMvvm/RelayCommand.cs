using System;
using System.Windows.Input;

namespace PPDMvvm
{
    /// <summary>
    /// デリゲートを実行するコマンドです。
    /// 実行可能かどうかの再評価は、WPF の CommandManager に任せます。
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action execute;
        private readonly Func<bool> canExecute;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="execute">実行する処理。</param>
        /// <param name="canExecute">実行できるかどうかを返す処理。</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        /// <summary>
        /// 実行できるかどうかが変わったときに発生します。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        /// <summary>
        /// 実行できるかどうかを返します。
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute();
        }

        /// <summary>
        /// 実行します。
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                execute();
            }
        }
    }

    /// <summary>
    /// 引数を受け取るデリゲートを実行するコマンドです。
    /// </summary>
    /// <typeparam name="T">引数の型。</typeparam>
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> execute;
        private readonly Func<T, bool> canExecute;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="execute">実行する処理。</param>
        /// <param name="canExecute">実行できるかどうかを返す処理。</param>
        public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
        {
            this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
            this.canExecute = canExecute;
        }

        /// <summary>
        /// 実行できるかどうかが変わったときに発生します。
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (canExecute != null)
                {
                    CommandManager.RequerySuggested += value;
                }
            }
            remove
            {
                if (canExecute != null)
                {
                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        /// <summary>
        /// 実行できるかどうかを返します。
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return canExecute == null || canExecute(Convert(parameter));
        }

        /// <summary>
        /// 実行します。
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                execute(Convert(parameter));
            }
        }

        private static T Convert(object parameter)
        {
            // MvvmLight と同じく、型が合わない引数（null を含む）は既定値として扱う
            return parameter is T value ? value : default(T);
        }
    }
}
