namespace AutoSquirrel
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    /// <summary>
    /// </summary>
    public static class CheckInternetConnection
    {
        /// <summary>
        /// Determines whether [is connected to internet].
        /// </summary>
        /// <returns><c>true</c> if [is connected to internet]; otherwise, <c>false</c>.</returns>
        public static bool IsConnectedToInternet()
        {
            try
            {
                int desc;
                return InternetGetConnectedState(out desc, 0);
            }
            catch
            {
                Debug.WriteLine("Problem with the connection to the Internet");
            }

            return false;
        }

        //Creating the extern function…
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int description, int reservedValue);
    }
}