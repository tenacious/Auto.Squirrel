namespace AutoSquirrel
{
    using System;
    using Caliburn.Micro;

    public class DialogHelper
    {
        //public static bool? ShowModalDialog(object viewModel, params Object[] param)
        //{
        //    var windowManager = new WindowManager();
        //    dynamic settings = new ExpandoObject();
        //    settings.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        //    return windowManager.ShowDialog(viewModel, null, settings);
        //}

        public static void ShowWindow<T>(params Object[] param) where T : class
        {
            var windowManager = new WindowManager();
            var viewModel = Activator.CreateInstance(typeof(T), param) as T;
            windowManager.ShowWindow(viewModel);
        }
    }
}