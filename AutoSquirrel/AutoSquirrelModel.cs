using Amazon.S3;
using Amazon.S3.Transfer;
using Caliburn.Micro;
using FluentValidation;
using FluentValidation.Results;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;
using NuGet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Serialization;

namespace AutoSquirrel
{

    /// 
    ///  M O D E L    C L A S S 
    /// 

    [DataContract]
    public class AutoSquirrelModel : PropertyChangedBaseValidable, GongSolutions.Wpf.DragDrop.IDropTarget
    {
        public AutoSquirrelModel()
        {
            PackageFiles = new ObservableCollection<ItemLink>();
            //AppId = "MyPackageId";
            //Title = "My Package";
            //Authors = "authors_name";
            //Description = "Package Description";
        }

        private string _appId;
        [DataMember]
        public string AppId
        {
            get { return _appId; }

            set
            {
                _appId = value;
                NotifyOfPropertyChange(() => AppId);
            }
        }

        private string _title;
        [DataMember]
        public string Title
        {
            get { return _title; }

            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        private string _mainExePath;
        [DataMember]
        public string MainExePath
        {
            get { return _mainExePath; }

            set
            {
                _mainExePath = value;
                NotifyOfPropertyChange(() => MainExePath);
            }
        }

        private string _description;
        [DataMember]
        public string Description
        {
            get { return _description; }

            set
            {
                _description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        private string _authors;
        [DataMember]
        public string Authors
        {
            get { return _authors; }

            set
            {
                _authors = value;
                NotifyOfPropertyChange(() => Authors);
            }
        }

        public string _squirrelOutputPath;
        [DataMember]
        public string SquirrelOutputPath
        {
            get { return _squirrelOutputPath; }

            set
            {
                _squirrelOutputPath = value;
                NotifyOfPropertyChange(() => SquirrelOutputPath);
            }
        }

        private string _nupkgOutputPath;
        [DataMember]
        public string NupkgOutputPath
        {
            get { return _nupkgOutputPath; }

            set
            {
                _nupkgOutputPath = value;
                NotifyOfPropertyChange(() => NupkgOutputPath);
            }
        }

        private ObservableCollection<ItemLink> _packageFiles = new ObservableCollection<ItemLink>();
        [DataMember]
        public ObservableCollection<ItemLink> PackageFiles
        {
            get
            {
                return _packageFiles;
            }
            set
            {
                _packageFiles = value;
                NotifyOfPropertyChange(() => PackageFiles);
            }
        }

        private string _version;
        [DataMember]
        public string Version
        {
            get { return _version; }

            set
            {
                _version = value;
                NotifyOfPropertyChange(() => Version);
            }
        }

        private bool _setVersionManually;
        [DataMember]
        public bool SetVersionManually
        {
            get { return _setVersionManually; }

            set
            {
                _setVersionManually = value;
                NotifyOfPropertyChange(() => SetVersionManually);
            }
        }


        [DataMember]
        public string CurrentFilePath { get; internal set; }


        public void SelectNupkgDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (Directory.Exists(NupkgOutputPath))
                dialog.SelectedPath = NupkgOutputPath;

            var result = dialog.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK) return;

            NupkgOutputPath = dialog.SelectedPath;
        }

        public void SelectOutputDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (Directory.Exists(SquirrelOutputPath))
                dialog.SelectedPath = SquirrelOutputPath;

            var result = dialog.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK) return;

            SquirrelOutputPath = dialog.SelectedPath;

        }


        /// <summary>
        /// Read the main exe version and set it as package versione 
        /// </summary>
        public void RefreshPackageVersion()
        {
            if (!File.Exists(MainExePath)) return;

            if (SetVersionManually) return;

            var versInfo = FileVersionInfo.GetVersionInfo(MainExePath);

            Version = versInfo.ProductVersion;
        }

        string newFolderName = "NEW FOLDER";

        private ICommand _addDirectoryCmd;
        public ICommand AddDirectoryCmd
        {
            get
            {
                return _addDirectoryCmd ??
                       (_addDirectoryCmd = new DelegateCommand(AddDirectory));
            }
        }

        private ICommand _removeItemCmd;
        public ICommand RemoveItemCmd
        {
            get
            {
                return _removeItemCmd ??
                       (_removeItemCmd = new DelegateCommand(RemoveItem));
            }
        }

        public void AddDirectory()
        {
            if (SelectedLink != null)
            {
                var validFolderName = GetValidName(newFolderName, SelectedLink.Children);

                SelectedLink.Children.Add(new ItemLink { OutputFilename = validFolderName, IsDirectory = true });
            }
            else
            {
                var validFolderName = GetValidName(newFolderName, PackageFiles);

                PackageFiles.Add(new ItemLink { OutputFilename = validFolderName, IsDirectory = true });
            }

        }

        private static string GetValidName(string newFolderName, ObservableCollection<ItemLink> children)
        {
            var folderName = newFolderName;

            var ex = children.FirstOrDefault(i => i.Filename == folderName);
            var index = 0;
            while (ex != null)
            {
                index++;
                folderName = newFolderName + " (" + index + ")";

                ex = children.FirstOrDefault(i => i.Filename == folderName);
            }

            return folderName;
        }

        public void RemoveItem()
        {
            if (SelectedLink == null) return;

            RemoveFromTreeview(SelectedLink);
        }


        public ItemLink SelectedLink { get; set; }
        public void SetSelectedItem(ItemLink item)
        {
            SelectedLink = item;
        }

        #region ICON

        private ICommand _selectIconCmd;
        public ICommand SelectIconCmd
        {
            get
            {
                return _selectIconCmd ??
                       (_selectIconCmd = new DelegateCommand(SelectIcon));
            }
        }

        public void SelectIcon()
        {
            var ofd = new System.Windows.Forms.OpenFileDialog
            {
                AddExtension = true,
                DefaultExt = ".ico",
                Filter = "ICON | *.ico"
            };

            var o = ofd.ShowDialog();

            if (o != System.Windows.Forms.DialogResult.OK || !File.Exists(ofd.FileName)) return;

            IconFilepath = ofd.FileName;
        }

        private string _iconFilepath;
        [DataMember]
        public string IconFilepath
        {
            get { return _iconFilepath; }

            set
            {
                _iconFilepath = value;
                NotifyOfPropertyChange(() => IconFilepath);
                NotifyOfPropertyChange(() => IconSource);
            }
        }

        public ImageSource IconSource
        {
            get
            {
                try
                {
                    return GetImageFromFilepath(IconFilepath);
                }
                catch
                {
                    //Todo - icona default
                    return null;
                }

            }
        }

        internal static BitmapImage GetImageFromFilepath(string path)
        {
            if (!File.Exists(path))
                return null;

            if (File.Exists(path))
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.None;
                bitmap.UriCachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                bitmap.EndInit();

                return bitmap;

            }

            return new BitmapImage();
        }
        #endregion

        #region Connection 
        //
        // 30/01/2016
        // Changed from multiple connection list, to a single connection
        //

        //private ICommand _addAmazonConnectionCmd;
        //public ICommand AddAmazonConnectionCmd
        //{
        //    get
        //    {
        //        return _addAmazonConnectionCmd ??
        //               (_addAmazonConnectionCmd = new DelegateCommand(() => AddNewConnection("amazon")));
        //    }
        //}

        //public void AddNewConnection(string s)
        //{
        //    WebConnectionBase vm = null;
        //    switch (s)
        //    {
        //        case "amazon":
        //            vm = new AmazonS3Connection();
        //            break;
        //    }

        //    if (vm == null) return;
        //    var vw = new WebConnectionEdit() { WindowStartupLocation = WindowStartupLocation.CenterScreen };
        //    vw.DataContext = vm;

        //    var rslt = vw.ShowDialog();

        //    if (rslt != true) return;

        //    WebConnections.Add(vm);
        //}

        //private ICommand _editCurrentConnection;
        //public ICommand EditCurrentConnectionCmd
        //{
        //    get
        //    {
        //        return _editCurrentConnection ??
        //               (_editCurrentConnection = new DelegateCommand(EditCurrentConnection));
        //    }
        //}

        //public void EditCurrentConnection()
        //{
        //    if (SelectedConnection == null) return;

        //    var vw = new WebConnectionEdit();

        //    vw.DataContext = SelectedConnection;

        //    var rslt = vw.ShowDialog();
        //}

        //private ICommand _deleteCurrentConnectionCmd;
        //public ICommand DeleteCurrentConnectionCmd
        //{
        //    get
        //    {
        //        return _deleteCurrentConnectionCmd ??
        //               (_deleteCurrentConnectionCmd = new DelegateCommand(DeleteCurrentConnection));
        //    }
        //}
        //public void DeleteCurrentConnection()
        //{
        //    if (SelectedConnection == null) return;

        //    if (WebConnections.Contains(SelectedConnection))
        //        WebConnections.Remove(SelectedConnection);
        //}

        //private ObservableCollection<WebConnectionBase> _webConnections = new ObservableCollection<WebConnectionBase>();
        //[DataMember]
        //public ObservableCollection<WebConnectionBase> WebConnections
        //{
        //    get
        //    {
        //        return _webConnections;
        //    }
        //    set
        //    {
        //        _webConnections = value;
        //        NotifyOfPropertyChange(() => WebConnections);
        //    }
        //}
        private List<string> _availableUploadLocation;
        public List<string> AvailableUploadLocation
        {
            get
            {
                if (_availableUploadLocation == null)
                {
                    _availableUploadLocation = new List<string>()
                    {
                        "Amazon S3",
                        "File System",
                    };
                }

                return _availableUploadLocation;
            }
        }

        private string _selectedConnectionString;
        [DataMember]
        public string SelectedConnectionString
        {
            get
            {
                return _selectedConnectionString;
            }
            set
            {
                if (_selectedConnectionString == value) return;
                UpdateSelectedConnection(value);
                _selectedConnectionString = value;
                NotifyOfPropertyChange(() => SelectedConnectionString);
            }
        }

        /// <summary>
        /// I keep in memory created WebConnectionBase, so if the user switch accidentaly the connection string , he don't lose inserted parameter
        /// </summary>

        [DataMember]
        internal List<WebConnectionBase> CachedConnection = new List<WebConnectionBase>();

        private void UpdateSelectedConnection(string connectionType)
        {
            if (string.IsNullOrWhiteSpace(connectionType)) return;

            if (CachedConnection == null)
                CachedConnection = new List<WebConnectionBase>();

            WebConnectionBase con = null;
            switch (connectionType)
            {
                case "Amazon S3":
                    {
                        con = CachedConnection.FirstOrDefault(c => c is AmazonS3Connection);

                        if (con == null)
                            con = new AmazonS3Connection();

                    }
                    break;

                case "File System":
                    {
                        con = CachedConnection.FirstOrDefault(c => c is FileSystemConnection);
                        if (con == null)
                            con = new FileSystemConnection();

                    }
                    break;
            }

            if (con != null && !CachedConnection.Contains(con))
                CachedConnection.Add(con);

            SelectedConnection = con;
        }

        private WebConnectionBase _selectedConnection;
        [DataMember]
        public WebConnectionBase SelectedConnection
        {
            get
            {
                return _selectedConnection;
            }
            set
            {
                _selectedConnection = value;
                NotifyOfPropertyChange(() => SelectedConnection);
            }
        }

        private ICommand _editConnectionCmd;
        public ICommand EditConnectionCmd
        {
            get
            {
                return _editConnectionCmd ??
                       (_editConnectionCmd = new DelegateCommand(EditCurrentConnection));
            }
        }

        public void EditCurrentConnection()
        {
            if (SelectedConnection == null) return;

            var vw = new WebConnectionEdit();

            vw.DataContext = SelectedConnection;

            var rslt = vw.ShowDialog();
        }



        private ObservableCollection<SingleFileUpload> _uploadQueue = new ObservableCollection<SingleFileUpload>();
        public ObservableCollection<SingleFileUpload> UploadQueue
        {
            get
            {
                return _uploadQueue;
            }
            set
            {
                _uploadQueue = value;
                NotifyOfPropertyChange(() => UploadQueue);
            }
        }

        private SingleFileUpload _selectedUploadItem;
        public SingleFileUpload SelectedUploadItem
        {
            get
            {
                return _selectedUploadItem;
            }
            set
            {
                _selectedUploadItem = value;
                NotifyOfPropertyChange(() => SelectedUploadItem);
            }
        }


        #endregion



        public void DragOver(IDropInfo dropInfo)
        {
            //  dropInfo.NotHandled = true;
            //dropInfo.Data
            //     if (dropInfo.Data is PupilViewModel)// && dropInfo.TargetItem is SchoolViewModel)
            //{

            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Copy;
            //}

        }

        /// <summary>
        /// ON DROP 
        /// </summary>
        public void Drop(IDropInfo dropInfo)
        {

            //
            // MOVE FILE INSIDE PACKAGE
            //

            var draggedItem = dropInfo.Data as ItemLink;
            var targetItem = dropInfo.TargetItem as ItemLink;

            if (draggedItem != null)
            {
                /* To handle file moving :
                 * 
                 * Step 1 - Remove item from treeview
                 * Step 2 - Add as child of target element
                 * Step 3 - I update the [OutputFilepath] property,  accordingly to current treeview status.
                 *
                 */

                dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
                dropInfo.Effects = DragDropEffects.Move;

                MoveItem(draggedItem, targetItem);
            }

            //
            //  FILE ADDED FROM FILE SYSTEM
            // 
            var dataObj = dropInfo.Data as DataObject;

            if (dataObj != null)
            {

                if (dataObj.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] files = (string[])dataObj.GetData(DataFormats.FileDrop);

                    foreach (string filePath in files)
                        AddFile(filePath, targetItem);
                }

                PackageFiles = OrderFileList(PackageFiles);
            }
        }

        internal static ObservableCollection<ItemLink> OrderFileList(ObservableCollection<ItemLink> packageFiles)
        {
            foreach (var node in packageFiles)
            {
                node.Children = OrderFileList(node.Children);
            }

            return new ObservableCollection<ItemLink>(packageFiles.OrderByDescending(n => n.IsDirectory).ThenBy(n => n.Filename));


        }

        private void AddFile(string filePath, ItemLink targetItem)
        {
            var isDir = false;
            FileAttributes fa = File.GetAttributes(filePath);
            if (fa != null && fa.HasFlag(FileAttributes.Directory))
                isDir = true;

            RemoveItemBySourceFilepath(filePath);

            var node = new ItemLink() { SourceFilepath = filePath, IsDirectory = isDir };

            ItemLink parent = targetItem;
            if (targetItem == null)
            {
                //Porto su root
                _packageFiles.Add(node);
            }
            else
            {
                if (!targetItem.IsDirectory)
                    parent = targetItem.GetParent(PackageFiles);

                if (parent != null)
                {
                    //Insert into treeview root
                    parent.Children.Add(node);
                }
                else
                {
                    //Insert into treeview root
                    _packageFiles.Add(node);
                }
            }

            if (isDir)
            {
                var dir = new DirectoryInfo(filePath);

                var files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                var subDirectory = dir.GetDirectories("*.*", SearchOption.TopDirectoryOnly);

                foreach (var f in files)
                    AddFile(f.FullName, node);

                foreach (var f in subDirectory)
                    AddFile(f.FullName, node);
            }
            else
            {
                // I keep the exe filepath, i'll read the version from this file.
                // 
                var ext = Path.GetExtension(filePath).ToLower();

                if (ext == ".exe")
                {
                    MainExePath = filePath;

                    RefreshPackageVersion();
                }
            }


        }

        private void MoveItem(ItemLink draggedItem, ItemLink targetItem)
        {
            //
            // Remove from current location 
            RemoveFromTreeview(draggedItem);


            //
            // Add to target position
            ItemLink parent = targetItem;
            if (targetItem == null)
            {
                //Porto su root
                _packageFiles.Add(draggedItem);
            }
            else
            {
                if (!targetItem.IsDirectory)
                    parent = targetItem.GetParent(PackageFiles);

                if (parent != null)
                {
                    //Insert into treeview root
                    parent.Children.Add(draggedItem);
                }
                else
                {
                    //Insert into treeview root
                    _packageFiles.Add(draggedItem);
                }
            }

            NotifyOfPropertyChange(() => PackageFiles);
        }


        private void RemoveFromTreeview(ItemLink item)
        {
            var parent = item.GetParent(PackageFiles);

            if (MainExePath != null && item.SourceFilepath != null && MainExePath.ToLower() == item.SourceFilepath.ToLower())
            {
                MainExePath = string.Empty;
                RefreshPackageVersion();
            }

            //
            // Element is in the treeview root.
            if (parent == null)
            {
                if (_packageFiles.Contains(item))
                    _packageFiles.Remove(item);

            }
            else
            {
                //Remove it from children list
                parent.Children.Remove(item);
            }
        }

        private void RemoveItemBySourceFilepath(string filepath)
        {
            var list = new List<ItemLink>();

            SearchNodeByFilepath(filepath, PackageFiles, list);

            foreach (var node in list)
                RemoveFromTreeview(node);
        }

        private static void SearchNodeByFilepath(string filepath, ObservableCollection<ItemLink> root, List<ItemLink> rslt)
        {
            foreach (var node in root)
            {
                if (node.SourceFilepath != null && filepath.ToLower() == node.SourceFilepath.ToLower())
                    rslt.Add(node);

                SearchNodeByFilepath(filepath, node.Children, rslt);
            }
        }

        /// <summary>
        /// 29/01/2015
        /// 1) Create update files list
        /// 2) Create queue upload list. Iterating file list foreach connection ( i can have multiple cloud storage )
        /// 3) Start async upload.
        /// </summary>
        internal void BeginUpdatedFiles(int mode)
        {
            // ? -> Set IsEnabled = false on GUI to prevent change during upload ? 

            var releasesPath = SquirrelOutputPath;

            if (!Directory.Exists(releasesPath))
                throw new Exception("Releases directory " + releasesPath + "not finded !");

            if (SelectedConnection == null)
                throw new Exception("No selected upload location !");

            /* I tried picking file to update, by their  LastWriteTime , but it doesn't works good. I don't know why.
             * 
             * So i just pick these file by their name
             *
             */

            var fileToUpdate = new List<string>()
            {

                "RELEASES",
                string.Format("{0}-{1}-delta.nupkg",AppId,Version),
            };

            if (mode == 0)
            {
                fileToUpdate.Add(string.Format("Setup.exe"));
                //     fileToUpdate.Add(string.Format("{0}-{1}-full.nupkg", AppId, Version));
            }




            var updatedFiles = new List<FileInfo>();

            foreach (var fp in fileToUpdate)
            {
                var ffp = releasesPath + fp;
                if (!File.Exists(ffp)) continue;

                updatedFiles.Add(new FileInfo(ffp));
            }

            if (UploadQueue == null)
                UploadQueue = new ObservableCollection<SingleFileUpload>();

            UploadQueue.Clear();

            var WebConnections = new List<WebConnectionBase>() { SelectedConnection };
            foreach (var connection in WebConnections)
            {
                foreach (var file in updatedFiles)
                {
                    UploadQueue.Add(new SingleFileUpload()
                    {
                        Filename = Path.GetFileName(file.FullName),
                        ConnectionName = connection.ConnectionName,
                        FileSize = BytesToString(file.Length),
                        Connection = connection,
                        FullPath = file.FullName,
                    });
                }
            }

            if (!CheckInternetConnection.IsConnectedToInternet())
                throw new Exception("Internet Connection not available");

            ProcessNextUploadFile();
        }

        static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private void ProcessNextUploadFile()
        {
            try
            {
                var current = UploadQueue.FirstOrDefault(u => u.UploadStatus == FileUploadStatus.Queued);

                if (current == null)
                    return;

                current.OnUploadCompleted += Current_OnUploadCompleted;

                current.StartUpload();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void Current_OnUploadCompleted(object sender, UploadCompleteEventArgs e)
        {
            var i = e.FileUploaded;

            i.OnUploadCompleted -= Current_OnUploadCompleted;

            Trace.WriteLine("Upload Complete " + i.Filename);

            //if (i != null && UploadQueue.Contains(i))
            //    UploadQueue.Remove(i);

            ProcessNextUploadFile();
        }


        /// <summary>
        /// Prima controllo correttezza del pattern poi controllo questo.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {

            var commonValid = new Validator().Validate(this);
            if (!commonValid.IsValid)
                return commonValid;

            return base.Validate();
        }


        private class Validator : AbstractValidator<AutoSquirrelModel>
        {
            public Validator()
            {
                RuleFor(c => c.AppId).NotEmpty();
                RuleFor(c => c.Title).NotEmpty();
                RuleFor(c => c.Description).NotEmpty();
                RuleFor(c => c.Version).NotEmpty();
                RuleFor(c => c.PackageFiles).NotEmpty();
                RuleFor(c => c.Authors).NotEmpty();
                RuleFor(c => c.SelectedConnectionString).NotEmpty();

            }
        }

    }

    public static class FileUtility
    {
        public static void SerializeToFile<TRet>(string filePath, TRet objectToSerialize)
        {
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }

            try
            {
                var serializer = new JsonSerializer();
                serializer.TypeNameHandling = TypeNameHandling.All;
                serializer.NullValueHandling = NullValueHandling.Ignore;
                using (StreamWriter sw = new StreamWriter(filePath))
                using (JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, objectToSerialize);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }

        }

        public static TRet Deserialize<TRet>(string filePath)
        {
            try
            {
                using (StreamReader file = File.OpenText(filePath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.TypeNameHandling = TypeNameHandling.All;

                    return (TRet)serializer.Deserialize(file, typeof(TRet));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }
            return default(TRet);
        }
    }


    public static class PathFolderHelper
    {
        public const string ProjectFileExtension = ".asproj";

        public const string ProgramName = "Auto.Squirrel";
        public static string FileDialogName = ProgramName + " | *" + ProjectFileExtension;
        public static string ProgramBaseDirectory = "\\" + ProgramName;
        private const string UserDataDirectory = "\\Data\\";
        private const string ProjectDirectory = "\\Projects\\";
        internal const string PackageDirectory = "\\Packages\\";
        internal const string ReleasesDirectory = "\\Releases\\";

        public static string GetMyDirectory(MyDirectory directory)
        {
            var folderPath = string.Empty;

            switch (directory)
            {
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


            if (string.IsNullOrWhiteSpace(folderPath))
                throw new NotImplementedException("GetMyFilepath");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            return folderPath;
        }



        /// <summary>
        /// Rimuove i caratteri non validi dalle stringhe
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        internal static string ValidateFilename(string filename)
        {
            var invalidChars =
                Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            var invalidReStr = String.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            return Regex.Replace(filename, invalidReStr, String.Empty);
        }

        /// <summary>
        /// Incrementa nome file se file è già esistente.
        /// </summary>
        public static string GetCorrectFilepath(string path, string filename, string fileExt)
        {
            var filePath = String.Format("{0}\\{1}.{2}", path, filename, fileExt);

            var fileC = 0;
            while (File.Exists(filePath))
            {
                filePath = String.Format("{0}\\{1}_{2}.{3}", path, filename, fileC, fileExt);
                fileC++;
            }

            return filePath;
        }

        internal static string GetProgramVersion()
        {
            var ver = Assembly.GetExecutingAssembly()
                             .GetName()
                             .Version;

            return string.Format("{0}.{1}.{2}", ver.Major, ver.Minor, ver.Build);
        }

        internal static Preference LoadUserPreference()
        {
            try
            {
                var path = GetMyDirectory(MyDirectory.Base) + "\\Preference.txt";

                if (File.Exists(path))
                {
                    var p = FileUtility.Deserialize<Preference>(path);

                    // Check if project files still exist.

                    var temp = p.LastOpenedProject.ToList();

                    p.LastOpenedProject.Clear();

                    foreach (var fp in temp)
                    {
                        if (File.Exists(fp))
                            p.LastOpenedProject.Add(fp);
                    }

                    return p;

                }

                return new Preference();
            }
            catch (Exception)
            {
                return new Preference();
            }
        }

        internal static void SavePreference(Preference userPreference)
        {
            try
            {
                var path = GetMyDirectory(MyDirectory.Base) + "\\Preference.txt";

                FileUtility.SerializeToFile(path, userPreference);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on saving preference !");
            }
        }
    }
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
    public class DelegateCommand : ICommand
    {
        #region Fields
        readonly System.Action _execute; readonly Predicate<object> _canExecute;
        #endregion
        // Fields 
        #region Constructors
        public DelegateCommand(System.Action execute) : this(execute, null) { }
        public DelegateCommand(System.Action execute, Predicate<object> canExecute)
        {
            if (execute == null) throw new ArgumentNullException("execute");
            _execute = execute; _canExecute = canExecute;
        }
        #endregion
        // Constructors 
        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }
        public event EventHandler CanExecuteChanged { add { CommandManager.RequerySuggested += value; } remove { CommandManager.RequerySuggested -= value; } }

        public void Execute(object parameter)
        {
            _execute();
        }
        #endregion
        // ICommand Members
    }


    public enum MyDirectory
    {
        Project,
        //CustomDir,
        Base,
        //PackageDir,
    }

    enum MyFilepath
    {
        Preference,
    }

}
