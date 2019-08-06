using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AutoSquirrel
{
    /// <summary>
    /// Check Internet Connection
    /// </summary>
    public static class CheckInternetConnection
    {
        /// <summary>
        /// Determines whether [is connected to internet].
        /// </summary>
        /// <returns><c>true</c> if [is connected to internet]; otherwise, <c>false</c>.</returns>
        public static bool IsConnectedToInternet()
        {
            try {
                return InternetGetConnectedState(out var desc, 0);
            } catch {
                Debug.WriteLine("Problem with the connection to the Internet");
            }

            return false;
        }

        //Creating the extern function…
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int description, int reservedValue);
    }
}
