using System;
using Caliburn.Micro;

namespace AutoSquirrel
{
    /// <summary>
    /// Dialog Helper
    /// </summary>
    public static class DialogHelper
    {
        /// <summary>
        /// Shows the window.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="param">The parameter.</param>
        public static void ShowWindow<T>(params object[] param) where T : class
        {
            var windowManager = new WindowManager();
            var viewModel = Activator.CreateInstance(typeof(T), param) as T;
            windowManager.ShowWindow(viewModel);
        }
    }
}
