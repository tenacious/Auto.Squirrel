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

namespace AutoSquirrel
{
    /// <summary>
    /// Auto Squirrel Model
    /// </summary>
    /// <seealso cref="AutoSquirrel.PropertyChangedBaseValidable"/>
    /// <seealso cref="GongSolutions.Wpf.DragDrop.IDropTarget"/>
    [DataContract]
    public class AutoSquirrelModel : PropertyChangedBaseValidable, IDropTarget
    {
        [DataMember]
        internal List<WebConnectionBase> CachedConnection = new List<WebConnectionBase>();

        private readonly ConnectionDiscoveryService connectionDiscoveryService = new ConnectionDiscoveryService();
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
        private ICommand _refreshVersionNumber;
        private ICommand _removeAllItemsCmd;
        private ICommand _removeItemCmd;
        private WebConnectionBase _selectedConnection;
        private string _selectedConnectionString;
        private SingleFileUpload _selectedUploadItem;
        private ICommand _selectIconCmd;
        private bool _setVersionManually;
        private string _splashFilepath;
        private string _squirrelOutputPath;
        private string _title;
        private ObservableCollection<SingleFileUpload> _uploadQueue = new ObservableCollection<SingleFileUpload>();
        private string _version;
        private string newFolderName = "NEW FOLDER";
        private ItemLink selectedItem = new ItemLink();
        private ICommand selectSplashCmd;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoSquirrelModel"/> class.
        /// </summary>
        public AutoSquirrelModel() => PackageFiles = new ObservableCollection<ItemLink>();

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
            get => _appId;

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
            get => _authors;

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
                _availableUploadLocation =
                    _availableUploadLocation ?? new List<string>(
                        connectionDiscoveryService.AvailableConnections.Select(
                            connection => connection.ConnectionName));

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
            get => _description;

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
            get => _iconFilepath;

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
                try {
                    return GetImageFromFilepath(IconFilepath);
                } catch {
                    //TODO -  default icon
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
            get => _mainExePath;

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
            get => _nupkgOutputPath;

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
            get => _packageFiles;

            set
            {
                _packageFiles = value;
                NotifyOfPropertyChange(() => PackageFiles);
            }
        }

        /// <summary>
        /// Gets the refresh version number.
        /// </summary>
        /// <value>The refresh version number.</value>
        public ICommand RefreshVersionNumber => _refreshVersionNumber ??
               (_refreshVersionNumber = new DelegateCommand(RefreshPackageVersion));

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
            get => _selectedConnection;

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
            get => _selectedConnectionString;

            set
            {
                if (_selectedConnectionString == value) {
                    return;
                }

                UpdateSelectedConnection(value);
                _selectedConnectionString = value;
                NotifyOfPropertyChange(() => SelectedConnectionString);
            }
        }

        /// <summary>
        /// Gets the selected item.
        /// </summary>
        /// <value>The selected item.</value>
        public ItemLink SelectedItem
        {
            get => selectedItem;

            set
            {
                selectedItem = value;
                NotifyOfPropertyChange(() => SelectedItem);
            }
        }

        /// <summary>
        /// Gets or sets the selected link.
        /// </summary>
        /// <value>The selected link.</value>
        public IList<ItemLink> SelectedLink { get; set; } = new List<ItemLink>();

        /// <summary>
        /// Gets or sets the selected upload item.
        /// </summary>
        /// <value>The selected upload item.</value>
        public SingleFileUpload SelectedUploadItem
        {
            get => _selectedUploadItem;

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
        /// Gets the select splash command.
        /// </summary>
        /// <value>The select splash command.</value>
        public ICommand SelectSplashCmd =>
                    selectSplashCmd ?? (selectSplashCmd = new DelegateCommand(SelectSplash));

        /// <summary>
        /// Gets or sets a value indicating whether [set version manually].
        /// </summary>
        /// <value><c>true</c> if [set version manually]; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool SetVersionManually
        {
            get => _setVersionManually;

            set
            {
                _setVersionManually = value;
                NotifyOfPropertyChange(() => SetVersionManually);
                RefreshPackageVersion();
            }
        }

        /// <summary>
        /// Gets or sets the splash filepath.
        /// </summary>
        /// <value>The splash filepath.</value>
        [DataMember]
        public string SplashFilepath
        {
            get => _splashFilepath;

            set
            {
                _splashFilepath = value;
                NotifyOfPropertyChange(() => SplashFilepath);
                NotifyOfPropertyChange(() => SplashSource);
            }
        }

        /// <summary>
        /// Gets the splash source.
        /// </summary>
        /// <value>The splash source.</value>
        public ImageSource SplashSource
        {
            get
            {
                try {
                    return GetImageFromFilepath(SplashFilepath);
                } catch {
                    //Todo - splasha default
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the squirrel output path.
        /// </summary>
        /// <value>The squirrel output path.</value>
        [DataMember]
        public string SquirrelOutputPath
        {
            get => _squirrelOutputPath;

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
            get => _title;

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
            get => _uploadQueue;

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
            get => _version;

            set
            {
                var test = value.Split('.');
                if (test.Length <= 3) {
                    _version = value;
                    NotifyOfPropertyChange(() => Version);
                } else {
                    if (MessageBox.Show("Please use a Semantic Versioning 2.0.0 standard for the version number i.e. Major.Minor.Build http://semver.org/", "Invalid Version", MessageBoxButton.OK) == MessageBoxResult.OK) {
                        RefreshPackageVersion();
                    }
                }
            }
        }

        /// <summary>
        /// Adds the directory.
        /// </summary>
        public void AddDirectory()
        {
            if (SelectedLink.Count != 1) {
                return;
            }

            var selectedLink = SelectedLink[0];
            if (selectedLink != null) {
                var validFolderName = GetValidName(newFolderName, selectedLink.Children);

                selectedLink.Children.Add(new ItemLink { OutputFilename = validFolderName, IsDirectory = true });
            } else {
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
        /// <param name="dropInfo"></param>
        public void Drop(IDropInfo dropInfo)
        {
            // MOVE FILE INSIDE PACKAGE

            var targetItem = dropInfo.TargetItem as ItemLink;

            if (dropInfo.Data is ItemLink draggedItem) {
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

            if (dropInfo.Data is DataObject dataObj) {
                if (dataObj.GetDataPresent(DataFormats.FileDrop)) {
                    foreach (var filePath in (string[])dataObj.GetData(DataFormats.FileDrop)) {
                        if (!filePath.Contains(".pdb") && !filePath.Contains(".nupkg") && !filePath.Contains(".vshost.")) {
                            AddFile(filePath, targetItem);
                        }
                    }
                }

                PackageFiles = OrderFileList(PackageFiles);
            }
        }

        /// <summary>
        /// Edits the current connection.
        /// </summary>
        public void EditCurrentConnection()
        {
            if (SelectedConnection == null) {
                return;
            }

            var vw = new WebConnectionEdit()
            {
                DataContext = SelectedConnection
            };
            _ = vw.ShowDialog();
        }

        /// <summary>
        /// Read the main exe version and set it as package version
        /// </summary>
        public void RefreshPackageVersion()
        {
            if (!File.Exists(MainExePath)) {
                return;
            }

            if (SetVersionManually) {
                return;
            }

            var versInfo = FileVersionInfo.GetVersionInfo(MainExePath);

            Version = $"{versInfo.ProductMajorPart}.{versInfo.ProductMinorPart}.{versInfo.ProductBuildPart}";
        }

        /// <summary>
        /// Removes all items.
        /// </summary>
        public void RemoveAllItems()
        {
            if (SelectedLink == null || SelectedLink.Count == 0) {
                return;
            }

            RemoveAllFromTreeview(SelectedLink[0]);
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        public void RemoveItem()
        {
            if (SelectedLink == null || SelectedLink?.Count == 0) {
                return;
            }

            foreach (var link in SelectedLink) {
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

            if (o != System.Windows.Forms.DialogResult.OK || !File.Exists(ofd.FileName)) {
                return;
            }

            IconFilepath = ofd.FileName;
        }

        /// <summary>
        /// Selects the nupkg directory.
        /// </summary>
        public void SelectNupkgDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (Directory.Exists(NupkgOutputPath)) {
                dialog.SelectedPath = NupkgOutputPath;
            }

            var result = dialog.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK) {
                return;
            }

            NupkgOutputPath = dialog.SelectedPath;
        }

        /// <summary>
        /// Selects the output directory.
        /// </summary>
        public void SelectOutputDirectory()
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();

            if (Directory.Exists(SquirrelOutputPath)) {
                dialog.SelectedPath = SquirrelOutputPath;
            }

            var result = dialog.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK) {
                return;
            }

            SquirrelOutputPath = dialog.SelectedPath;
        }

        /// <summary>
        /// Handles the splash screen selection.
        /// </summary>
        public void SelectSplash()
        {
            var ofd = new System.Windows.Forms.OpenFileDialog
            {
                AddExtension = true,
                DefaultExt = ".gif",
                Filter = "GIF | *.gif"
            };

            var o = ofd.ShowDialog();

            if (o != System.Windows.Forms.DialogResult.OK || !File.Exists(ofd.FileName)) {
                return;
            }

            SplashFilepath = ofd.FileName;
        }

        /// <summary>
        /// Sets the selected item.
        /// </summary>
        /// <param name="item">The item.</param>
        public void SetSelectedItem(IList<ItemLink> item)
        {
            SelectedLink = item;
            SelectedItem = SelectedLink.FirstOrDefault() ?? new ItemLink();
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            var commonValid = new Validator().Validate(this);
            if (!commonValid.IsValid) {
                return commonValid;
            }

            return base.Validate();
        }

        internal static BitmapImage GetImageFromFilepath(string path)
        {
            if (!File.Exists(path)) {
                return null;
            }

            if (File.Exists(path)) {
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
            foreach (var node in packageFiles) {
                node.Children = OrderFileList(node.Children);
            }

            return new ObservableCollection<ItemLink>(packageFiles.OrderByDescending(n => n.IsDirectory).ThenBy(n => n.Filename));
        }

        /// <summary>
        /// 29/01/2015
        /// 1) Create update files list
        /// 2) Create queue upload list. Iterating file list foreach connection ( i can have multiple
        /// cloud storage )
        /// 3) Start async upload.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <exception cref="Exception"></exception>
        internal void BeginUpdatedFiles(int mode)
        {
            // ? -> Set IsEnabled = false on GUI to prevent change during upload ?

            var releasesPath = SquirrelOutputPath;

            if (!Directory.Exists(releasesPath)) {
                throw new Exception("Releases directory " + releasesPath + "not found !");
            }

            if (SelectedConnection == null) {
                throw new Exception("No selected upload location !");
            }

            /* I tried picking file to update, by their  LastWriteTime , but it doesn't works good. I don't know why.
             *
             * So i just pick these file by their name
             *
             */

            var fileToUpdate = new List<string>()
            {
                "RELEASES",
                $"{AppId}-{Version}-delta.nupkg",
            };

            if (mode == 0) {
                fileToUpdate.Add($"{AppId}-{Version}-full.nupkg");
                fileToUpdate.Add("Setup.exe");
            }

            var updatedFiles = new List<FileInfo>();

            foreach (var fp in fileToUpdate) {
                var ffp = releasesPath + fp;
                if (!File.Exists(ffp)) {
                    continue;
                }

                updatedFiles.Add(new FileInfo(ffp));
            }

            UploadQueue = UploadQueue ?? new ObservableCollection<SingleFileUpload>();

            UploadQueue.Clear();

            foreach (var connection in new List<WebConnectionBase>() { SelectedConnection }) {
                foreach (var file in updatedFiles) {
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

            ProcessNextUploadFile();
        }

        private static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0) {
                return "0" + suf[0];
            }

            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private static string GetValidName(string newFolderName, ObservableCollection<ItemLink> children)
        {
            var folderName = newFolderName;

            var ex = children.FirstOrDefault(i => i.Filename == folderName);
            var index = 0;
            while (ex != null) {
                index++;
                folderName = newFolderName + " (" + index + ")";

                ex = children.FirstOrDefault(i => i.Filename == folderName);
            }

            return folderName;
        }

        private static void SearchNodeByFilepath(string filepath, ObservableCollection<ItemLink> root, List<ItemLink> rslt)
        {
            foreach (var node in root) {
                if (node.SourceFilepath != null && string.Equals(filepath, node.SourceFilepath, StringComparison.CurrentCultureIgnoreCase)) {
                    rslt.Add(node);
                }

                SearchNodeByFilepath(filepath, node.Children, rslt);
            }
        }

        private void AddFile(string filePath, ItemLink targetItem)
        {
            var isDir = false;
            var fa = File.GetAttributes(filePath);
            if (fa.HasFlag(FileAttributes.Directory)) {
                isDir = true;
            }

            RemoveItemBySourceFilepath(filePath);

            var node = new ItemLink() { SourceFilepath = filePath, IsDirectory = isDir };

            var parent = targetItem;
            if (targetItem == null) {
                //Add to root
                _packageFiles.Add(node);
            } else {
                if (!targetItem.IsDirectory) {
                    parent = targetItem.GetParent(PackageFiles);
                }

                if (parent != null) {
                    //Insert into treeview root
                    parent.Children.Add(node);
                } else {
                    //Insert into treeview root
                    _packageFiles.Add(node);
                }
            }

            if (isDir) {
                var dir = new DirectoryInfo(filePath);

                var files = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                var subDirectory = dir.GetDirectories("*.*", SearchOption.TopDirectoryOnly);

                foreach (var f in files) {
                    AddFile(f.FullName, node);
                }

                foreach (var f in subDirectory) {
                    AddFile(f.FullName, node);
                }
            } else {
                // I keep the exe filepath, i'll read the version from this file.
                var ext = Path.GetExtension(filePath).ToLower();

                if (ext == ".exe") {
                    var nodeParent = node.GetParent(PackageFiles);
                    if (nodeParent == null) {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        if (string.IsNullOrWhiteSpace(AppId)) {
                            AppId = fileName;
                        }
                        if (string.IsNullOrWhiteSpace(Title)) {
                            Title = fileName;
                        }

                        MainExePath = filePath;
                        var versInfo = FileVersionInfo.GetVersionInfo(MainExePath);
                        if (string.IsNullOrWhiteSpace(Description)) {
                            Description = versInfo.Comments;
                        }

                        if (string.IsNullOrWhiteSpace(Authors)) {
                            Authors = versInfo.CompanyName;
                        }

                        RefreshPackageVersion();
                    }
                }
            }
        }

        private void Current_OnUploadCompleted(object sender, UploadCompleteEventArgs e)
        {
            var i = e.FileUploaded;

            i.OnUploadCompleted -= Current_OnUploadCompleted;

            Trace.WriteLine("Upload Complete " + i.Filename);

            ProcessNextUploadFile();
        }

        private void MoveItem(ItemLink draggedItem, ItemLink targetItem)
        {
            // Remove from current location
            RemoveFromTreeview(draggedItem);

            // Add to target position
            var parent = targetItem;
            if (targetItem == null) {
                //Porto su root
                _packageFiles.Add(draggedItem);
            } else {
                if (!targetItem.IsDirectory) {
                    parent = targetItem.GetParent(PackageFiles);
                }

                if (parent != null) {
                    //Insert into treeview root
                    parent.Children.Add(draggedItem);
                } else {
                    //Insert into treeview root
                    _packageFiles.Add(draggedItem);
                }
            }

            NotifyOfPropertyChange(() => PackageFiles);
        }

        private void ProcessNextUploadFile()
        {
            try {
                var current = UploadQueue.FirstOrDefault(u => u.UploadStatus == FileUploadStatus.Queued);

                if (current == null) {
                    return;
                }

                current.OnUploadCompleted += Current_OnUploadCompleted;

                current.StartUpload();
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString());
            }
        }

        private void RemoveAllFromTreeview(ItemLink item)
        {
            var parent = item.GetParent(PackageFiles);

            // Element is in the treeview root.
            if (parent == null) {
                _packageFiles.Clear();
                NotifyOfPropertyChange(() => PackageFiles);
            } else {
                //Remove it from children list
                parent.Children.Clear();
            }
            MainExePath = string.Empty;
            RefreshPackageVersion();
        }

        private void RemoveFromTreeview(ItemLink item)
        {
            var parent = item.GetParent(PackageFiles);

            if (MainExePath != null && item.SourceFilepath != null && string.Equals(MainExePath, item.SourceFilepath, StringComparison.CurrentCultureIgnoreCase)) {
                MainExePath = string.Empty;
                RefreshPackageVersion();
            }

            // Element is in the treeview root.
            if (parent == null) {
                if (_packageFiles.Contains(item)) {
                    _packageFiles.Remove(item);
                }
            } else {
                //Remove it from children list
                parent.Children.Remove(item);
            }
        }

        private void RemoveItemBySourceFilepath(string filepath)
        {
            var list = new List<ItemLink>();

            SearchNodeByFilepath(filepath, PackageFiles, list);

            foreach (var node in list) {
                RemoveFromTreeview(node);
            }
        }

        /// <summary>
        /// I keep in memory created WebConnectionBase, so if the user switch accidentally the
        /// connection string , he don't lose inserted parameter
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        private void UpdateSelectedConnection(string connectionType)
        {
            if (string.IsNullOrWhiteSpace(connectionType)) {
                return;
            }

            // Instantiate cache if null
            CachedConnection = CachedConnection ?? new List<WebConnectionBase>();

            // Retrieve cached connection or take new isntance from connection service
            var con =
                CachedConnection.FirstOrDefault(c => c.ConnectionName == connectionType) ??
                connectionDiscoveryService.GetByName(connectionType);

            // Cache connection if not cached already
            if (con != null && !CachedConnection.Contains(con)) {
                CachedConnection.Add(con);
            }

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
