using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;

namespace AutoSquirrel
{
    /// <summary>
    /// Path Folder Helper
    /// </summary>
    public static class PathFolderHelper
    {
        /// <summary>
        /// The program name
        /// </summary>
        public const string ProgramName = "Auto.Squirrel";

        /// <summary>
        /// The project file extension
        /// </summary>
        public const string ProjectFileExtension = ".asproj";

        /// <summary>
        /// The file dialog name
        /// </summary>
        public static string FileDialogName = ProgramName + " | *" + ProjectFileExtension;

        /// <summary>
        /// The program base directory
        /// </summary>
        public static string ProgramBaseDirectory = "\\" + ProgramName;

        internal const string PackageDirectory = "\\Packages\\";
        internal const string ReleasesDirectory = "\\Releases\\";
        private const string ProjectDirectory = "\\Projects\\";
        private const string UserDataDirectory = "\\Data\\";

        /// <summary>
        /// Gets the correct filepath.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="fileExt">The file ext.</param>
        /// <returns></returns>
        public static string GetCorrectFilepath(string path, string filename, string fileExt)
        {
            var filePath = $"{path}\\{filename}.{fileExt}";

            var fileC = 0;
            while (File.Exists(filePath)) {
                filePath = $"{path}\\{filename}_{fileC}.{fileExt}";
                fileC++;
            }

            return filePath;
        }

        /// <summary>
        /// Gets my directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException">GetMyFilepath</exception>
        public static string GetMyDirectory(MyDirectory directory)
        {
            var folderPath = string.Empty;

            switch (directory) {
                //case MyDirectory.PackageDir:
                //    folderPath = GetMyDirectory(MyDirectory.Base) + PackageDirectory;
                //    break;

                case MyDirectory.Project:
                    folderPath = GetMyDirectory(MyDirectory.Base) + ProjectDirectory;
                    break;

                case MyDirectory.Base:
                    folderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + ProgramBaseDirectory;
                    break;
            }

            if (string.IsNullOrWhiteSpace(folderPath)) {
                throw new NotImplementedException("GetMyFilepath");
            }

            if (!Directory.Exists(folderPath)) {
                Directory.CreateDirectory(folderPath);
            }

            return folderPath;
        }

        internal static string GetProgramVersion()
        {
            var ver = Assembly.GetExecutingAssembly()
                             .GetName()
                             .Version;

            return $"{ver.Major}.{ver.Minor}.{ver.Build}";
        }

        internal static Preference LoadUserPreference()
        {
            try {
                var path = GetMyDirectory(MyDirectory.Base) + "\\Preference.txt";

                if (File.Exists(path)) {
                    var p = FileUtility.Deserialize<Preference>(path);

                    // Check if project files still exist.

                    var temp = p.LastOpenedProject.ToList();

                    p.LastOpenedProject.Clear();

                    foreach (var fp in temp) {
                        if (File.Exists(fp)) {
                            p.LastOpenedProject.Add(fp);
                        }
                    }

                    return p;
                }

                return new Preference();
            } catch (Exception) {
                return new Preference();
            }
        }

        internal static void SavePreference(Preference userPreference)
        {
            try {
                var path = GetMyDirectory(MyDirectory.Base) + "\\Preference.txt";

                FileUtility.SerializeToFile(path, userPreference);
            } catch (Exception) {
                MessageBox.Show("Error on saving preference !");
            }
        }

        internal static string ValidateFilename(string filename)
        {
            var invalidChars = Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = $@"([{invalidChars}]*\.+$)|([{invalidChars}]+)";
            return Regex.Replace(filename, invalidReStr, string.Empty);
        }
    }
}
