using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoSquirrel
{
    /// <summary>
    /// </summary>
    public static class IconHelper
    {
        /// <summary>
        /// The file attribute directory
        /// </summary>
        public const uint FILE_ATTRIBUTE_DIRECTORY = 0x00000010;

        /// <summary>
        /// The shgfi icon
        /// </summary>
        public const uint SHGFI_ICON = 0x000000100;

        /// <summary>
        /// The shgfi largeicon
        /// </summary>
        public const uint SHGFI_LARGEICON = 0x000000000;

        /// <summary>
        /// The shgfi openicon
        /// </summary>
        public const uint SHGFI_OPENICON = 0x000000002;

        /// <summary>
        /// The shgfi smallicon
        /// </summary>
        public const uint SHGFI_SMALLICON = 0x000000001;

        /// <summary>
        /// The shgfi usefileattributes
        /// </summary>
        public const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        private static readonly Dictionary<string, ImageSource> _largeIconCache = new Dictionary<string, ImageSource>();

        //public static class IconManager
        //{
        private static readonly Dictionary<string, ImageSource> _smallIconCache = new Dictionary<string, ImageSource>();

        /// <summary>
        /// </summary>
        public enum FolderType
        {
            /// <summary>
            /// The closed
            /// </summary>
            Closed,

            /// <summary>
            /// The open
            /// </summary>
            Open
        }

        /// <summary>
        /// </summary>
        public enum IconSize
        {
            /// <summary>
            /// The large
            /// </summary>
            Large,

            /// <summary>
            /// The small
            /// </summary>
            Small
        }

        //}
        /// <summary>
        /// Get an icon for a given filename
        /// </summary>
        /// <param name="fileName">any filename</param>
        /// <param name="large">16x16 or 32x32 icon</param>
        /// <returns>null if path is null, otherwise - an icon</returns>
        public static ImageSource FindIconForFilename(string fileName, bool large)
        {
            var extension = Path.GetExtension(fileName);
            if (extension == null)
            {
                return null;
            }

            var cache = large ? _largeIconCache : _smallIconCache;
            if (cache.TryGetValue(extension, out var icon))
            {
                return icon;
            }

            icon = IconReader.GetFileIcon(fileName, large ? IconReader.IconSize.Large : IconReader.IconSize.Small, false).ToImageSource();
            cache.Add(extension, icon);
            return icon;
        }

        /// <summary>
        /// Gets the folder icon.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="folderType">Type of the folder.</param>
        /// <returns></returns>
        public static Icon GetFolderIcon(IconSize size, FolderType folderType)
        {
            // Need to add size check, although errors generated at present!
            uint flags = SHGFI_ICON | SHGFI_USEFILEATTRIBUTES;

            if (FolderType.Open == folderType)
            {
                flags += SHGFI_OPENICON;
            }
            if (IconSize.Small == size)
            {
                flags += SHGFI_SMALLICON;
            }
            else
            {
                flags += SHGFI_LARGEICON;
            }

            // Get the folder icon
            var shfi = new SHFILEINFO();

            var res = SHGetFileInfo(Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                FILE_ATTRIBUTE_DIRECTORY,
                out shfi,
                (uint)Marshal.SizeOf(shfi),
                flags);

            if (res == IntPtr.Zero)
            {
                throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            }

            // Load the icon from an HICON handle
            Icon.FromHandle(shfi.hIcon);

            // Now clone the icon, so that it can be successfully stored in an ImageList
            var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();

            DestroyIcon(shfi.hIcon);        // Cleanup

            return icon;
        }

        /// <summary>
        /// Shes the get file information.
        /// </summary>
        /// <param name="pszPath">The PSZ path.</param>
        /// <param name="dwFileAttributes">The dw file attributes.</param>
        /// <param name="psfi">The psfi.</param>
        /// <param name="cbFileInfo">The cb file information.</param>
        /// <param name="uFlags">The u flags.</param>
        /// <returns></returns>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbFileInfo, uint uFlags);

        /// <summary>
        /// To the image source.
        /// </summary>
        /// <param name="icon">The icon.</param>
        /// <returns></returns>
        public static ImageSource ToImageSource(this Icon icon)
        {
            if (icon == null)
            {
                return null;
            }

            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct SHFILEINFO
        {
            /// <summary>
            /// The h icon
            /// </summary>
            public IntPtr hIcon;

            /// <summary>
            /// The i icon
            /// </summary>
            public int iIcon;

            /// <summary>
            /// The dw attributes
            /// </summary>
            public uint dwAttributes;

            /// <summary>
            /// The sz display name
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            /// <summary>
            /// The sz type name
            /// </summary>
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        /// <summary>
        /// Provides static methods to read system icons for both folders and files.
        /// </summary>
        /// <example>
        /// <code>
        /// IconReader.GetFileIcon("c:\\general.xls");
        /// </code>
        /// </example>
        private static class IconReader
        {
            /// <summary>
            /// Options to specify the size of icons to return.
            /// </summary>
            public enum IconSize
            {
                /// <summary>
                /// Specify large icon - 32 pixels by 32 pixels.
                /// </summary>
                Large = 0,

                /// <summary>
                /// Specify small icon - 16 pixels by 16 pixels.
                /// </summary>
                Small = 1
            }

            /// <summary>
            /// Returns an icon for a given file - indicated by the name parameter.
            /// </summary>
            /// <param name="name">Pathname for file.</param>
            /// <param name="size">Large or small</param>
            /// <param name="linkOverlay">Whether to include the link icon</param>
            /// <returns>System.Drawing.Icon</returns>
            public static Icon GetFileIcon(string name, IconSize size, bool linkOverlay)
            {
                var shfi = new Shell32.Shfileinfo();
                var flags = Shell32.ShgfiIcon | Shell32.ShgfiUsefileattributes;
                if (linkOverlay)
                {
                    flags += Shell32.ShgfiLinkoverlay;
                }
                /* Check the size specified for return. */
                if (IconSize.Small == size)
                {
                    flags += Shell32.ShgfiSmallicon;
                }
                else
                {
                    flags += Shell32.ShgfiLargeicon;
                }

                Shell32.SHGetFileInfo(name,
                    Shell32.FileAttributeNormal,
                    ref shfi,
                    (uint)Marshal.SizeOf(shfi),
                    flags);

                // Copy (clone) the returned icon to a new object, thus allowing us to clean-up properly
                var icon = (Icon)Icon.FromHandle(shfi.hIcon).Clone();
                User32.DestroyIcon(shfi.hIcon);     // Cleanup
                return icon;
            }
        }

        /// <summary>
        /// Wraps necessary Shell32.dll structures and functions required to retrieve Icon Handles
        /// using SHGetFileInfo. Code courtesy of MSDN Cold Rooster Consulting case study.
        /// </summary>
        private static class Shell32
        {
            public const uint FileAttributeNormal = 0x00000080;
            public const uint ShgfiIcon = 0x000000100;
            public const uint ShgfiLargeicon = 0x000000000;

            // get icon
            public const uint ShgfiLinkoverlay = 0x000008000;

            // put a link overlay on icon get large icon
            public const uint ShgfiSmallicon = 0x000000001;

            // get small icon
            public const uint ShgfiUsefileattributes = 0x000000010;

            private const int MaxPath = 256;

            // use passed dwFileAttribute
            [DllImport("Shell32.dll")]
            public static extern IntPtr SHGetFileInfo(
                string pszPath,
                uint dwFileAttributes,
                ref Shfileinfo psfi,
                uint cbFileInfo,
                uint uFlags
                );

            [StructLayout(LayoutKind.Sequential)]
            public struct Shfileinfo
            {
                private const int Namesize = 80;
                public readonly IntPtr hIcon;
                private readonly int iIcon;
                private readonly uint dwAttributes;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MaxPath)]
                private readonly string szDisplayName;

                [MarshalAs(UnmanagedType.ByValTStr, SizeConst = Namesize)]
                private readonly string szTypeName;
            };
        }

        /// <summary>
        /// Wraps necessary functions imported from User32.dll. Code courtesy of MSDN Cold Rooster
        /// Consulting example.
        /// </summary>
        private static class User32
        {
            /// <summary>
            /// Provides access to function required to delete handle. This method is used internally
            /// and is not required to be called separately.
            /// </summary>
            /// <param name="hIcon">Pointer to icon handle.</param>
            /// <returns>N/A</returns>
            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);
        }
    }
}