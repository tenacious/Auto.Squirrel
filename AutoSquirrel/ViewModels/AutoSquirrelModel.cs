namespace AutoSquirrel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Cache;
    using System.Runtime.Serialization;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using FluentValidation;
    using FluentValidation.Results;
    using GongSolutions.Wpf.DragDrop;

    /// <summary>
    /// Auto Squirrel Model
    /// </summary>
    /// <seealso cref="AutoSquirrel.PropertyChangedBaseValidable"/>
    /// <seealso cref="GongSolutions.Wpf.DragDrop.IDropTarget"/>
    [DataContract]
    public class AutoSquirrelModel : PropertyChangedBaseValidable, GongSolutions.Wpf.DragDrop.IDropTarget
    {
        public string _squirrelOutputPath;

        [DataMember]
        internal List<WebConnectionBase> CachedConnection = new List<WebConnectionBase>();

        private ICommand _addDirectoryCmd;
        private string _appId;
        private string _authors;
        private List<string> _availableUploadLocation;
        private string _description;
        private ICommand _editConnectionCmd;
        private string _iconFilepath;
        private string _mainExePath;
        private string _nupkgOutputPath;
        private ObservableCollection<ItemLink> _packageFiles = new ObservableCollection<ItemLink>();
        private ICommand _removeAllItemsCmd;
        private ICommand _removeItemCmd;
        private WebConnectionBase _selectedConnection;
        private string _selectedConnectionString;
        private SingleFileUpload _selectedUploadItem;
        private ICommand _selectIconCmd;
        private bool _setVersionManually;
        private string _title;
        private ObservableCollection<SingleFileUpload> _uploadQueue = new ObservableCollection<SingleFileUpload>();
        private string _version;
        private string newFolderName = "NEW FOLDER";

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSquirrelModel"/> class.
        /// </summary>
        public AutoSquirrelModel()
        {
            PackageFiles = new ObservableCollection<ItemLink>();

            //AppId = "MyPackageId";
            //Title = "My Package";
            //Authors = "authors_name";
            //Description = "Package Description";
        }

        /// <summary>
        /// Gets the add directory command.
        /// </summary>
        /// <value>The add directory command.</value>
        public ICommand AddDirectoryCmd => _addDirectoryCmd ??
       (_addDirectoryCmd = new DelegateCommand(AddDirectory));

        /// <summary>
        /// Gets or sets the application identifier.
        /// </summary>
        /// <value>The application identifier.</value>
        [DataMember]
        public string AppId
        {
            get
            {
                return _appId;
            }

            set
            {
                _appId = value;
                NotifyOfPropertyChange(() => AppId);
            }
        }

        /// <summary>
        /// Gets or sets the authors.
        /// </summary>
        /// <value>The authors.</value>
        [DataMember]
        public string Authors
        {
            get
            {
                return _authors;
            }

            set
            {
                _authors = value;
                NotifyOfPropertyChange(() => Authors);
            }
        }

        /// <summary>
        /// Gets the available upload location.
        /// </summary>
        /// <value>The available upload location.</value>
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

        /// <summary>
        /// Gets the current file path.
        /// </summary>
        /// <value>The current file path.</value>
        [DataMember]
        public string CurrentFilePath { get; internal set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        [DataMember]
        public string Description
        {
            get
            {
                return _description;
            }

            set
            {
                _description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        /// <summary>
        /// Gets the edit connection command.
        /// </summary>
        /// <value>The edit connection command.</value>
        public ICommand EditConnectionCmd => _editConnectionCmd ??
       (_editConnectionCmd = new DelegateCommand(EditCurrentConnection));

        /// <summary>
        /// Gets or sets the icon filepath.
        /// </summary>
        /// <value>The icon filepath.</value>
        [DataMember]
        public string IconFilepath
        {
            get
            {
                return _iconFilepath;
            }

            set
            {
                _iconFilepath = value;
                NotifyOfPropertyChange(() => IconFilepath);
                NotifyOfPropertyChange(() => IconSource);
            }
        }

        /// <summary>
        /// Gets the icon source.
        /// </summary>
        /// <value>The icon source.</value>
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

        /// <summary>
        /// Gets or sets the main executable path.
        /// </summary>
        /// <value>The main executable path.</value>
        [DataMember]
        public string MainExePath
        {
            get
            {
                return _mainExePath;
            }

            set
            {
                _mainExePath = value;
                NotifyOfPropertyChange(() => MainExePath);
            }
        }

        /// <summary>
        /// Gets or sets the nupkg output path.
        /// </summary>
        /// <value>The nupkg output path.</value>
        [DataMember]
        public string NupkgOutputPath
        {
            get
            {
                return _nupkgOutputPath;
            }

            set
            {
                _nupkgOutputPath = value;
                NotifyOfPropertyChange(() => NupkgOutputPath);
            }
        }

        /// <summary>
        /// Gets or sets the package files.
        /// </summary>
        /// <value>The package files.</value>
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

        /// <summary>
        /// Gets the remove all items command.
        /// </summary>
        /// <value>The remove all items command.</value>
        public ICommand RemoveAllItemsCmd => _removeAllItemsCmd ??
               (_removeAllItemsCmd = new DelegateCommand(RemoveAllItems));

        /// <summary>
        /// Gets the remove item command.
        /// </summary>
        /// <value>The remove item command.</value>
        public ICommand RemoveItemCmd => _removeItemCmd ??
       (_removeItemCmd = new DelegateCommand(RemoveItem));

        /// <summary>
        /// Gets or sets the selected connection.
        /// </summary>
        /// <value>The selected connection.</value>
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

        /// <summary>
        /// Gets or sets the selected connection string.
        /// </summary>
        /// <value>The selected connection string.</value>
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
        /// Gets or sets the selected link.
        /// </summary>
        /// <value>The selected link.</value>
        public IList<ItemLink> SelectedLink { get; set; }

        /// <summary>
        /// Gets or sets the selected upload item.
        /// </summary>
        /// <value>The selected upload item.</value>
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

        /// <summary>
        /// Gets the select icon command.
        /// </summary>
        /// <value>The select icon command.</value>
        public ICommand SelectIconCmd => _selectIconCmd ??
       (_selectIconCmd = new DelegateCommand(SelectIcon));

        /// <summary>
        /// Gets or sets a value indicating whether [set version manually].
        /// </summary>
        /// <value><c>true</c> if [set version manually]; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool SetVersionManually
        {
            get
            {
                return _setVersionManually;
            }

            set
            {
                _setVersionManually = value;
                NotifyOfPropertyChange(() => SetVersionManually);
            }
        }

        /// <summary>
        /// Gets or sets the squirrel output path.
        /// </summary>
        /// <value>The squirrel output path.</value>
        [DataMember]
        public string SquirrelOutputPath
        {
            get
            {
                return _squirrelOutputPath;
            }

            set
            {
                _squirrelOutputPath = value;
                NotifyOfPropertyChange(() => SquirrelOutputPath);
            }
        }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        [DataMember]
        public string Title
        {
            get
            {
                return _title;
            }

            set
            {
                _title = value;
                NotifyOfPropertyChange(() => Title);
            }
        }

        /// <summary>
        /// Gets or sets the upload queue.
        /// </summary>
        /// <value>The upload queue.</value>
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

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>The version.</value>
        [DataMember]
        public string Version
        {
            get
            {
                return _version;
            }

            set
            {
                _version = value;
                NotifyOfPropertyChange(() => Version);
            }
        }

        /// <summary>
        /// Adds the directory.
        /// </summary>
        public void AddDirectory()
        {
            if (SelectedLink.Count != 1) return;
            var selectedLink = SelectedLink[0];
            if (selectedLink != null)
            {
                var validFolderName = GetValidName(newFolderName, selectedLink.Children);

                selectedLink.Children.Add(new ItemLink { OutputFilename = validFolderName, IsDirectory = true });
            }
            else
            {
                var validFolderName = GetValidName(newFolderName, PackageFiles);

                PackageFiles.Add(new ItemLink { OutputFilename = validFolderName, IsDirectory = true });
            }
        }

        /// <summary>
        /// Updates the current drag state.
        /// </summary>
        /// <param name="dropInfo">Information about the drag.</param>
        /// <remarks>
        /// To allow a drop at the current drag position, the <see
        /// cref="P:GongSolutions.Wpf.DragDrop.DropInfo.Effects"/> property on <paramref
        /// name="dropInfo"/> should be set to a value other than <see
        /// cref="F:System.Windows.DragDropEffects.None"/> and <see
        /// cref="P:GongSolutions.Wpf.DragDrop.DropInfo.Data"/> should be set to a non-null value.
        /// </remarks>
        public void DragOver(IDropInfo dropInfo)
        {
            dropInfo.DropTargetAdorner = DropTargetAdorners.Highlight;
            dropInfo.Effects = DragDropEffects.Copy;
        }

        /// <summary>
        /// ON DROP
        /// </summary>
        public void Drop(IDropInfo dropInfo)
        {
            // MOVE FILE INSIDE PACKAGE

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

            // FILE ADDED FROM FILE SYSTEM
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

        /// <summary>
        /// Edits the current connection.
        /// </summary>
        public void EditCurrentConnection()
        {
            if (SelectedConnection == null) return;

            var vw = new WebConnectionEdit();

            vw.DataContext = SelectedConnection;

            var rslt = vw.ShowDialog();
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

        /// <summary>
        /// Removes all items.
        /// </summary>
        public void RemoveAllItems()
        {
            if (SelectedLink?.Count == 0) return;

            RemoveAllFromTreeview(SelectedLink[0]);
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        public void RemoveItem()
        {
            if (SelectedLink?.Count == 0) return;
            foreach (var link in SelectedLink)
            {
                RemoveFromTreeview(link);
            }
        }

        /// <summary>
        /// Selects the icon.
        /// </summary>
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

        /// <summary>
        /// Selects the nupkg directory.
        /// </summary>
        public void SelectNupkgDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (Directory.Exists(NupkgOutputPath))
                dialog.SelectedPath = NupkgOutputPath;

            var result = dialog.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK) return;

            NupkgOutputPath = dialog.SelectedPath;
        }

        /// <summary>
        /// Selects the output directory.
        /// </summary>
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
        /// Sets the selected item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void SetSelectedItem(IList<ItemLink> item)
        {
            SelectedLink = item;
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

        internal static ObservableCollection<ItemLink> OrderFileList(ObservableCollection<ItemLink> packageFiles)
        {
            foreach (var node in packageFiles)
            {
                node.Children = OrderFileList(node.Children);
            }

            return new ObservableCollection<ItemLink>(packageFiles.OrderByDescending(n => n.IsDirectory).ThenBy(n => n.Filename));
        }

        /// <summary>
        /// 29/01/2015
        /// 1) Create update files list
        /// 2) Create queue upload list. Iterating file list foreach connection ( i can have multiple
        ///    cloud storage )
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

                // fileToUpdate.Add(string.Format("{0}-{1}-full.nupkg", AppId, Version));
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

        private static String BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
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

        // 30/01/2016 Changed from multiple connection list, to a single connection

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

        // if (vm == null) return; var vw = new WebConnectionEdit() { WindowStartupLocation =
        // WindowStartupLocation.CenterScreen }; vw.DataContext = vm;

        // var rslt = vw.ShowDialog();

        // if (rslt != true) return;

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

        // var vw = new WebConnectionEdit();

        // vw.DataContext = SelectedConnection;

        private static void SearchNodeByFilepath(string filepath, ObservableCollection<ItemLink> root, List<ItemLink> rslt)
        {
            foreach (var node in root)
            {
                if (node.SourceFilepath != null && filepath.ToLower() == node.SourceFilepath.ToLower())
                    rslt.Add(node);

                SearchNodeByFilepath(filepath, node.Children, rslt);
            }
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
                var ext = Path.GetExtension(filePath).ToLower();

                if (ext == ".exe")
                {
                    MainExePath = filePath;

                    RefreshPackageVersion();
                }
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

        private void MoveItem(ItemLink draggedItem, ItemLink targetItem)
        {
            // Remove from current location
            RemoveFromTreeview(draggedItem);

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

        private void RemoveAllFromTreeview(ItemLink item)
        {
            var parent = item.GetParent(PackageFiles);

            // Element is in the treeview root.
            if (parent == null)
            {
                _packageFiles.Clear();
                NotifyOfPropertyChange(() => PackageFiles);
            }
            else
            {
                //Remove it from children list
                parent.Children.Clear();
            }
            MainExePath = string.Empty;
            RefreshPackageVersion();
        }

        private void RemoveFromTreeview(ItemLink item)
        {
            var parent = item.GetParent(PackageFiles);

            if (MainExePath != null && item.SourceFilepath != null && MainExePath.ToLower() == item.SourceFilepath.ToLower())
            {
                MainExePath = string.Empty;
                RefreshPackageVersion();
            }

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

        //    var rslt = vw.ShowDialog();
        //}
        /// <summary>
        /// I keep in memory created WebConnectionBase, so if the user switch accidentaly the
        /// connection string , he don't lose inserted parameter
        /// </summary>
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
}