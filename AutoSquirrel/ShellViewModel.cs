using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Caliburn.Micro;
using FluentValidation;
using FluentValidation.Results;
using GongSolutions.Wpf.DragDrop;
using NuGet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static AutoSquirrel.IconHelper;
using Amazon;
using System.Windows.Threading;
using System.Threading;
using Amazon.S3.Util;
using System.Security.Cryptography;
using System.Globalization;

namespace AutoSquirrel
{
    public class ShellViewModel : ViewAware
    {
        public AutoSquirrelModel _model;
        public AutoSquirrelModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                NotifyOfPropertyChange(() => Model);
            }
        }
        public Preference UserPreference;

        /// <summary>
        /// Ctor
        /// </summary>
        public ShellViewModel()
        {
            Model = new AutoSquirrelModel();

            UserPreference = PathFolderHelper.LoadUserPreference();

            var last = UserPreference.LastOpenedProject.LastOrDefault();

            if (!string.IsNullOrEmpty(last) && File.Exists(last))
                OpenProject(last);

        }


        ///
        /// M E T H O D S 
        /// 

        public void CreateNewProject()
        {
            var rslt = MessageBox.Show("Save current project ?", "New Project", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (rslt == MessageBoxResult.Cancel) return;

            if (rslt == MessageBoxResult.Yes)
                Save();

            Model = new AutoSquirrelModel();

        }

        public void OpenProject()
        {
            try
            {
                var ofd = new System.Windows.Forms.OpenFileDialog
                {
                    AddExtension = true,
                    DefaultExt = PathFolderHelper.ProjectFileExtension,
                    Filter = PathFolderHelper.FileDialogName
                };

                var iniDir = PathFolderHelper.GetMyDirectory(MyDirectory.Project);
                if (!string.IsNullOrWhiteSpace(iniDir))
                    ofd.InitialDirectory = iniDir;

                var o = ofd.ShowDialog();

                if (o != System.Windows.Forms.DialogResult.OK || !File.Exists(ofd.FileName)) return;

                OpenProject(ofd.FileName);

                //Save last folder path
            }
            catch (Exception exception)
            {
                MessageBox.Show("Loading File Error, file no more supported", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.None);
            }
        }

        public void OpenProject(string filepath)
        {
            try
            {
                if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
                {
                    MessageBox.Show("This file doesn't exist : " + filepath, "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.None);
                    return;
                }

                FilePath = filepath;

                var m = FileUtility.Deserialize<AutoSquirrelModel>(filepath);

                if (m == null) return;

                Model = m;

                Model.PackageFiles = AutoSquirrelModel.OrderFileList(Model.PackageFiles);

                Model.RefreshPackageVersion();

                AddLastProject(filepath);

            }
            catch (Exception exception)
            {
                MessageBox.Show("Loading File Error, file no more supported", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.None);
            }
        }

        public string FilePath
        {
            get
            {
                return Model.CurrentFilePath;
            }
            set
            {
                Model.CurrentFilePath = value;
                NotifyOfPropertyChange(() => FilePath);
            }
        }

        public void SaveAs()
        {
            var previousFilePath = FilePath;

            try
            {
                var saveFileDialog = new System.Windows.Forms.SaveFileDialog
                {
                    DefaultExt = PathFolderHelper.ProjectFileExtension,
                    AddExtension = true,
                    Filter = PathFolderHelper.FileDialogName,
                };

                // todo : usare cartella di salvataggio.
                var iniDir = PathFolderHelper.GetMyDirectory(MyDirectory.Project);

                if (!string.IsNullOrWhiteSpace(iniDir))
                    saveFileDialog.InitialDirectory = iniDir;

                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                //Singleton.Preference.DefaultProjectDir = Path.GetDirectoryName(saveFileDialog.FileName);

                //Singleton.SavePreference();

                FilePath = saveFileDialog.FileName;

                Save();

                //return true;
            }
            catch (Exception)
            {
                MessageBox.Show("Error on saving");

                FilePath = previousFilePath;

                //return false;
            }
        }

        bool _isSaved;

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                SaveAs();
                return;
            }

            var filename = Path.GetFileNameWithoutExtension(FilePath);

            var baseDir = Path.GetDirectoryName(FilePath);

            Model.NupkgOutputPath = baseDir + Path.DirectorySeparatorChar + filename + "_files" + PathFolderHelper.PackageDirectory;
            Model.SquirrelOutputPath = baseDir + Path.DirectorySeparatorChar + filename + "_files" + PathFolderHelper.ReleasesDirectory;

            if (!Directory.Exists(Model.NupkgOutputPath))
                Directory.CreateDirectory(Model.NupkgOutputPath);

            if (!Directory.Exists(Model.SquirrelOutputPath))
                Directory.CreateDirectory(Model.SquirrelOutputPath);

            FileUtility.SerializeToFile(FilePath, Model);

            Trace.WriteLine("FILE SAVED ! : " + FilePath);

            _isSaved = true;

            AddLastProject(FilePath);

            NotifyOfPropertyChange(() => WindowTitle);
        }

        private void AddLastProject(string filePath)
        {
            var existing = UserPreference.LastOpenedProject.Where(p => p.ToLower() == filePath.ToLower()).ToList();

            foreach (var fp in existing)
                UserPreference.LastOpenedProject.Remove(fp);

            UserPreference.LastOpenedProject.Add(filePath);

            PathFolderHelper.SavePreference(UserPreference);
        }

        public string WindowTitle
        {
            get
            {
                var fp = "New Project" + "*";
                if (!string.IsNullOrWhiteSpace(FilePath))
                    fp = Path.GetFileNameWithoutExtension(FilePath);

                return string.Format("{0} {1} - {2}", PathFolderHelper.ProgramName, PathFolderHelper.GetProgramVersion(), fp);
            }
        }

        /// <summary>
        /// Show/Hide Busy indicatory
        /// </summary>
        private bool _isBusy;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }

            set
            {
                _isBusy = value;
                NotifyOfPropertyChange(() => IsBusy);
            }
        }

        /// <summary>
        /// 
        /// 1) Check field validity
        /// 2) Create Nuget package
        /// 3) Squirrel relasify
        /// 4) Publish to amazon the updated file
        ///     ( to get the update file , search the timedate > of building time )
        ///     
        /// - Possibly in async way..
        /// - Must be callable from command line, so i can optionally start this process from at the end of visual studio release build
        /// </summary>
        public void PublishPackageComplete()
        {
            _publishMode = 0;
            PublishPackage();
        }
        public void PublishPackageOnlyUpdate()
        {
            _publishMode = 1;
            PublishPackage();
        }
        private int _publishMode;

        internal BackgroundWorker ActiveBackgroungWorker;

        public void PublishPackage()
        {
            try
            {
                if (ActiveBackgroungWorker != null && ActiveBackgroungWorker.IsBusy)
                {
                    Trace.TraceError("You shouldn't be here !");
                    return;
                }

                Model.RefreshPackageVersion();

                Trace.WriteLine("START PUBLISHING ! : " + Model.Title);

                // 1) Check validity  
                //var validatingMessage = Model.Validate();
                if (!Model.IsValid)
                    throw new Exception("Package Details are invalid or incomplete !");

                if (Model.SelectedConnection == null || !Model.SelectedConnection.IsValid)
                    throw new Exception("Selected connection details are not valid !");

                Trace.WriteLine("DATA VALIDATE - OK ! ");

                Save();

                // I proceed only if i created the project .asproj file and directory
                // I need existing directory to create the packages.

                if (!_isSaved)
                    return;

                IsBusy = true;

                ActiveBackgroungWorker = new BackgroundWorker() { WorkerReportsProgress = true };

                ActiveBackgroungWorker.DoWork += ActiveBackgroungWorker_DoWork;
                ActiveBackgroungWorker.RunWorkerCompleted += PackageCreationCompleted;
                ActiveBackgroungWorker.ProgressChanged += ActiveBackgroungWorker_ProgressChanged;

                ActiveBackgroungWorker.RunWorkerAsync(this);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error on publishing", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// Called on package created.
        /// Start the upload.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackageCreationCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            ActiveBackgroungWorker = null;

            if (e.Error != null)
            {
                //todo : Manage generated error
                return;
            }

            if (e.Cancelled) return;

            // Start uploading generated files.
            Model.BeginUpdatedFiles(_publishMode);

        }

        private void ActiveBackgroungWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //todo : Update busy indicator information.
        }

        private static void ActiveBackgroungWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var shellVm = e.Argument as ShellViewModel;

            if (shellVm == null) return;

            var model = shellVm.Model;
            var bgWorker = shellVm.ActiveBackgroungWorker;

            if (model == null || bgWorker == null) return;

            // Create Nuget Package from package treeview.
            var nugetPackagePath = model.CreateNugetPackage();
            Trace.WriteLine("CREATED NUGET PACKAGE to : " + model.NupkgOutputPath);

            // Releasify 
            SquirrelReleasify(nugetPackagePath, model.SquirrelOutputPath);
            Trace.WriteLine("CREATED SQUIRREL PACKAGE to : " + model.SquirrelOutputPath);

        }

        private static void SquirrelReleasify(string nugetPackagePath, string squirrelOutputPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            startInfo.FileName = @"tools\Squirrel.exe";

            var cmd = @" -releasify " + nugetPackagePath + " -releaseDir " + squirrelOutputPath;

            //if (File.Exists(Model.IconFilepath))
            //    cmd += " -setupIcon " + Model.IconFilepath;

            startInfo.Arguments = cmd;

            using (Process exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }




    }

    /// <summary>
    /// This class contains all information about WebConncetion uploading.
    /// Information for user : Credentials are stored in clear format.
    /// </summary>
    [DataContract]
    public class AmazonS3Connection : WebConnectionBase
    {
        public AmazonS3Connection()
        {
            ConnectionName = "Amazon S3";
        }

        //http://docs.aws.amazon.com/awscloudtrail/latest/userguide/cloudtrail-s3-bucket-naming-requirements.html
        private string _bucketName;
        [DataMember]
        public string BucketName
        {
            get
            {
                return _bucketName;
            }

            set
            {
                _bucketName = value;
                if (_bucketName != null)
                    _bucketName = _bucketName.ToLower().Replace(" ", string.Empty);

                NotifyOfPropertyChange(() => BucketName);
                NotifyOfPropertyChange(() => SetupDownloadUrl);

            }
        }

        private string _accessKey;
        [DataMember]
        public string AccessKey
        {
            get
            {
                return _accessKey;
            }

            set
            {
                _accessKey = value;
                NotifyOfPropertyChange(() => AccessKey);
            }
        }

        private string _secretAccessKey;
        [DataMember]
        public string SecretAccessKey
        {
            get
            {
                return _secretAccessKey;
            }

            set
            {
                _secretAccessKey = value;
                NotifyOfPropertyChange(() => SecretAccessKey);
            }
        }

        private string _regionName;
        [DataMember]
        public string RegionName
        {
            get
            {
                return _regionName;
            }

            set
            {
                _regionName = value;
                NotifyOfPropertyChange(() => RegionName);
                NotifyOfPropertyChange(() => SetupDownloadUrl);
            }
        }

        public string SetupDownloadUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(BucketName) || string.IsNullOrWhiteSpace(RegionName))
                    return "Missing Parameter";

                return "https://s3-" + GetRegion().SystemName + ".amazonaws.com/" + BucketName.ToLower() + "/Setup.exe";
            }
        }



        private List<string> _availableRegionList;
        public List<string> AvailableRegionList
        {
            get
            {
                if (_availableRegionList == null)
                {
                    _availableRegionList = new List<string>();

                    foreach (var r in RegionEndpoint.EnumerableAllRegions)
                        _availableRegionList.Add(r.DisplayName);

                }

                return _availableRegionList;
            }
        }

        internal RegionEndpoint GetRegion()
        {
            return RegionEndpoint.EnumerableAllRegions.FirstOrDefault(r => r.DisplayName == RegionName);
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


        private class Validator : AbstractValidator<AmazonS3Connection>
        {
            public Validator()
            {
                //RuleFor(c => c.ConnectionName).NotEmpty();
                RuleFor(c => c.RegionName).NotEmpty();
                RuleFor(c => c.SecretAccessKey).NotEmpty();
                RuleFor(c => c.AccessKey).NotEmpty();
                RuleFor(c => c.BucketName).Must(CheckBucketName).WithState(x => "Bucket Name not valid ! See Amazon SDK documentation");
            }

            private static bool CheckBucketName(string bucketName)
            {
                if (string.IsNullOrWhiteSpace(bucketName) || bucketName.Contains(" "))
                    return false;

                return true;
            }
        }



    }

    /// <summary>
    /// This class contains all information about WebConncetion uploading.
    /// Information for user : Credentials are stored in clear format.
    /// </summary>
    [DataContract]
    public class FileSystemConnection : WebConnectionBase
    {
        public FileSystemConnection()
        {
            ConnectionName = "File System";
        }

        private string _fileSystemPath;
        [DataMember]
        public string FileSystemPath
        {
            get
            {
                return _fileSystemPath;
            }

            set
            {
                _fileSystemPath = value;

                NotifyOfPropertyChange(() => FileSystemPath);
                NotifyOfPropertyChange(() => SetupDownloadUrl);

            }
        }

        //private string _accessKey;
        //[DataMember]
        //public string AccessKey
        //{
        //    get
        //    {
        //        return _accessKey;
        //    }

        //    set
        //    {
        //        _accessKey = value;
        //        NotifyOfPropertyChange(() => AccessKey);
        //    }
        //}

        //private string _secretAccessKey;
        //[DataMember]
        //public string SecretAccessKey
        //{
        //    get
        //    {
        //        return _secretAccessKey;
        //    }

        //    set
        //    {
        //        _secretAccessKey = value;
        //        NotifyOfPropertyChange(() => SecretAccessKey);
        //    }
        //}

        public string SetupDownloadUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FileSystemPath))
                    return "Missing Parameter";

                return FileSystemPath + "/Setup.exe";
            }
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


        private class Validator : AbstractValidator<FileSystemConnection>
        {
            public Validator()
            {
                RuleFor(c => c.FileSystemPath).NotEmpty();
            }
        }


    }

    //[DataContract]
    public class WebConnectionBase : PropertyChangedBaseValidable
    {
        private string _connectionName;
        [DataMember]
        public string ConnectionName
        {
            get
            {
                return _connectionName;
            }

            set
            {
                _connectionName = value;
                NotifyOfPropertyChange(() => ConnectionName);
            }
        }

    }


    /// <summary>
    /// Used in Upload queue list.
    /// I don't need serialization for this class.
    /// </summary>
    [DataContract]
    public class SingleFileUpload : PropertyChangedBase
    {

        private string _filename;
        [DataMember]
        public string Filename
        {
            get
            {
                return _filename;
            }

            set
            {
                _filename = value;
                NotifyOfPropertyChange(() => Filename);
            }
        }

        private string _connection;
        [DataMember]
        public string ConnectionName
        {
            get
            {
                return _connection;
            }

            set
            {
                _connection = value;
                NotifyOfPropertyChange(() => ConnectionName);
            }
        }


        private string _fileSize;
        [DataMember]
        public string FileSize
        {
            get
            {
                return _fileSize;
            }

            set
            {
                _fileSize = value;
                NotifyOfPropertyChange(() => FileSize);
            }
        }

        private FileUploadStatus _uploadStatus;
        public FileUploadStatus UploadStatus
        {
            get
            {
                return _uploadStatus;
            }

            set
            {
                _uploadStatus = value;
                NotifyOfPropertyChange(() => UploadStatus);
                NotifyOfPropertyChange(() => FormattedStatus);
            }
        }

        public string FormattedStatus
        {
            get
            {
                return UploadStatus.ToString();
            }
        }

        private double _progressPercentage;
        [DataMember]
        public double ProgressPercentage
        {
            get
            {
                return _progressPercentage;
            }

            set
            {
                _progressPercentage = value;
                NotifyOfPropertyChange(() => ProgressPercentage);
            }
        }

        public string FullPath { get; internal set; }
        public WebConnectionBase Connection { get; internal set; }

        public event EventHandler<UploadCompleteEventArgs> OnUploadCompleted;

        private void RequesteUploadComplete(UploadCompleteEventArgs uploadEvent)
        {
            UploadStatus = FileUploadStatus.Completed;
            ProgressPercentage = 100;

            var handler = OnUploadCompleted;
            if (handler != null)
                handler(null, uploadEvent);
        }



        TransferUtility fileTransferUtility;

        internal void StartUpload()
        {
            var amazonCon = Connection as AmazonS3Connection;

            if (amazonCon != null)
            {
                var amazonClient = new AmazonS3Client(amazonCon.AccessKey, amazonCon.SecretAccessKey, amazonCon.GetRegion());

                fileTransferUtility = new TransferUtility(amazonClient);

                if (!(AmazonS3Util.DoesS3BucketExist(amazonClient, amazonCon.BucketName)))
                {
                    CreateABucket(amazonClient, amazonCon.BucketName);
                }

                var uploadRequest =
                    new TransferUtilityUploadRequest
                    {
                        BucketName = amazonCon.BucketName,
                        FilePath = FullPath,
                        CannedACL = S3CannedACL.PublicRead,
                    };

                uploadRequest.UploadProgressEvent += uploadRequest_UploadPartProgressEvent;

                fileTransferUtility.UploadAsync(uploadRequest);


                Trace.WriteLine("Start Upload : " + FullPath);
            }
        }



        static void CreateABucket(IAmazonS3 client, string bucketName)
        {
            PutBucketRequest putRequest1 = new PutBucketRequest
            {

                BucketName = bucketName,
                UseClientRegion = true
            };

            PutBucketResponse response1 = client.PutBucket(putRequest1);

            Trace.WriteLine("Creating a bucket " + bucketName);

        }

        void uploadRequest_UploadPartProgressEvent(
          object sender, UploadProgressArgs e)
        {
            ProgressPercentage = e.PercentDone;

            if (e.PercentDone == 100)
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {

                    RequesteUploadComplete(new UploadCompleteEventArgs(this));
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(
                      DispatcherPriority.Background,
                      new System.Action(() =>
                      {

                          RequesteUploadComplete(new UploadCompleteEventArgs(this));
                      }));
                }
            }
        }
    }

    public enum FileUploadStatus
    {
        Queued,
        InProgress,
        Completed,
    }

    public class UploadCompleteEventArgs : EventArgs
    {
        public UploadCompleteEventArgs(SingleFileUpload sfu)
        {
            FileUploaded = sfu;
        }

        public SingleFileUpload FileUploaded { get; internal set; }
    }
    /// <summary>
    /// Extend PropertyChangedBase with Validation Behaviour
    /// </summary>
    public class PropertyChangedBaseValidable : PropertyChangedBase, IDataErrorInfo
    {

        public string Error
        {
            get { return GetError(Validate()); }
        }

        public string this[string columnName]
        {
            get
            {
                var __ValidationResults = Validate();
                if (__ValidationResults == null) return string.Empty;
                var __ColumnResults = __ValidationResults.Errors.FirstOrDefault(x => string.Compare(x.PropertyName, columnName, true) == 0);
                return __ColumnResults != null ? __ColumnResults.ErrorMessage : string.Empty;
            }
        }

        public bool IsValid
        {
            get { return Validate().IsValid; }
        }

        public virtual ValidationResult Validate()
        {
            return new ValidationResult();
        }

        public static string GetError(ValidationResult result)
        {
            var __ValidationErrors = new StringBuilder();
            foreach (var validationFailure in result.Errors)
            {
                __ValidationErrors.Append(validationFailure.ErrorMessage);
                __ValidationErrors.Append(Environment.NewLine);
            }
            return __ValidationErrors.ToString();
        }


    }

    [DataContract]
    public class ItemLink : PropertyChangedBase
    {
        /// <summary>
        /// Filepath of linked source file.
        /// Absolute ?
        /// </summary>
        [DataMember]
        public string SourceFilepath { get; set; }

        [DataMember]
        public bool IsDirectory { get; set; }

        private string _filename;
        [DataMember]
        public string Filename
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(OutputFilename))
                    return OutputFilename;

                if (!string.IsNullOrWhiteSpace(SourceFilepath))
                    return Path.GetFileName(SourceFilepath);

                return _filename;
            }
            internal set
            {
                _filename = value;
            }
        }

        private static string GetDirectoryName(string relativeOutputPath)
        {
            string[] directories = relativeOutputPath.Split(new List<char> { Path.DirectorySeparatorChar }.ToArray(), StringSplitOptions.RemoveEmptyEntries);

            return directories.LastOrDefault();
        }


        public ImageSource FileIcon
        {
            get
            {
                try
                {
                    Icon icon = null;

                    if (IsDirectory && IsExpanded)
                        icon = IconHelper.GetFolderIcon(IconSize.Large, FolderType.Open);
                    else if (IsDirectory && !IsExpanded)
                        icon = IconHelper.GetFolderIcon(IconSize.Large, FolderType.Closed);
                    else
                    {
                        if (File.Exists(SourceFilepath))
                            icon = Icon.ExtractAssociatedIcon(SourceFilepath);
                        else
                            return IconHelper.FindIconForFilename(Path.GetFileName(SourceFilepath), true);

                    }
                    if (icon == null) return null;

                    return icon.ToImageSource();
                }
                catch
                {
                    //Todo - icona default
                    return null;
                }

            }
        }

        #region Data

        static readonly ItemLink DummyChild = new ItemLink();

        [DataMember]
        ObservableCollection<ItemLink> _children = new ObservableCollection<ItemLink>();


        //bool _isExpanded;
        bool _isSelected;

        #endregion // Data

        #region Presentation Members

        #region Children

        /// <summary>
        /// Returns the logical child items of this object.
        /// </summary>
        public ObservableCollection<ItemLink> Children
        {
            get { return _children; }

            set
            {
                _children = value;
                NotifyOfPropertyChange(() => Children);
            }
        }

        #endregion // Children

        #region HasLoadedChildren

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild
        {
            get { return this.Children.Count == 1 && this.Children[0] == DummyChild; }
        }

        #endregion // HasLoadedChildren

        #region IsExpanded

        private bool _isExpanded { get; set; }
        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is expanded.
        /// </summary>
        [DataMember]
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value != _isExpanded)
                {
                    _isExpanded = value;
                    NotifyOfPropertyChange(() => IsExpanded);
                    NotifyOfPropertyChange(() => FileIcon);
                }

                //// Expand all the way up to the root.
                //if (_isExpanded && _parent != null)
                //    _parent.IsExpanded = true;

                // Lazy load the child items, if necessary.
                if (this.HasDummyChild)
                {
                    this.Children.Remove(DummyChild);
                    this.LoadChildren();
                }
            }
        }

        #endregion // IsExpanded

        #region IsSelected

        /// <summary>
        /// Gets/sets whether the TreeViewItem 
        /// associated with this object is selected.
        /// </summary>
        [DataMember]
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    NotifyOfPropertyChange(() => IsSelected);
                }
            }
        }

        [DataMember]
        public string OutputFilename { get; internal set; }

        /// <summary>
        /// Fixed folder. Can't remove or move.
        /// </summary>
        [DataMember]
        public bool IsRootBase { get; internal set; }

        #endregion // IsSelected

        #region LoadChildren

        // This is used to create the DummyChild instance.
        public ItemLink()
        {
        }
        /// <summary>
        /// Invoked when the child items need to be loaded on demand.
        /// Subclasses can override this to populate the Children collection.
        /// </summary>
        protected virtual void LoadChildren()
        {
        }

        #endregion // LoadChildren


        public ItemLink GetParent(ObservableCollection<ItemLink> root)
        {
            foreach (var node in root)
            {
                var p = FindParent(this, node);
                if (p != null)
                    return p;
            }

            return null;
        }

        private static ItemLink FindParent(ItemLink link, ItemLink node)
        {
            if (node.Children != null)
            {
                if (node.Children.Contains(link))
                    return node;

                foreach (var child in node.Children)
                {
                    var p = FindParent(link, child);
                    if (p != null)
                        return p;
                }
            }


            return null;
        }
        //Qui perdo il riferimento su deserilizzazione/serializzazione.
        //private ItemLink _parent;
        //[DataMember]
        //public ItemLink Parent
        //{
        //    get { return _parent; }
        //    set
        //    {
        //        _parent = value;
        //        NotifyOfPropertyChange(() => Parent);
        //    }
        //}


        #endregion // Presentation Members

    }



    [DataContract]
    public class Preference
    {
        [DataMember]
        public List<string> LastOpenedProject = new List<string>();
    }

    public static class CheckInternetConnection
    {
        //Creating the extern function…
        [DllImport("wininet.dll")]
        private extern static bool InternetGetConnectedState(out int description, int reservedValue);

        //Creating a function that uses the API function…
        public static bool IsConnectedToInternet()
        {
            try
            {
                int desc;
                return InternetGetConnectedState(out desc, 0);
            }
            catch
            {
                Debug.WriteLine("Problema nel determinare connessione internet");
            }

            return false;
        }
    }

}
