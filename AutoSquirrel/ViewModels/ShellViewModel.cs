namespace AutoSquirrel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Input;
    using Caliburn.Micro;
    using NuGet;

    /// <summary>
    /// Shell View Model
    /// </summary>
    /// <seealso cref="Caliburn.Micro.ViewAware"/>
    public class ShellViewModel : ViewAware
    {
        internal BackgroundWorker ActiveBackgroungWorker;
        private ICommand _abortPackageCreationCmd;
        private bool _abortPackageFlag;
        private string _currentPackageCreationStage;
        private bool _isBusy;
        private bool _isSaved;
        private AutoSquirrelModel _model;
        private int _publishMode;
        private Process exeProcess;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellViewModel"/> class.
        /// </summary>
        public ShellViewModel()
        {
            this.Model = new AutoSquirrelModel();

            this.UserPreference = PathFolderHelper.LoadUserPreference();

            var last = this.UserPreference.LastOpenedProject.LastOrDefault();

            if (!string.IsNullOrEmpty(last) && File.Exists(last))
            {
                OpenProject(last);
            }
        }

        /// <summary>
        /// Gets the abort package creation command.
        /// </summary>
        /// <value>The abort package creation command.</value>
        public ICommand AbortPackageCreationCmd => this._abortPackageCreationCmd ??
       (this._abortPackageCreationCmd = new DelegateCommand(this.AbortPackageCreation));

        /// <summary>
        /// Gets or sets the current package creation stage.
        /// </summary>
        /// <value>The current package creation stage.</value>
        public string CurrentPackageCreationStage
        {
            get => this._currentPackageCreationStage;

            set
            {
                this._currentPackageCreationStage = value;
                NotifyOfPropertyChange(() => this.CurrentPackageCreationStage);
            }
        }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>The file path.</value>
        public string FilePath
        {
            get => this.Model.CurrentFilePath;

            set
            {
                this.Model.CurrentFilePath = value;
                NotifyOfPropertyChange(() => this.FilePath);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value><c>true</c> if this instance is busy; otherwise, <c>false</c>.</value>
        public bool IsBusy
        {
            get => this._isBusy;

            set
            {
                this._isBusy = value;
                NotifyOfPropertyChange(() => this.IsBusy);
            }
        }

        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public AutoSquirrelModel Model
        {
            get => this._model;

            set
            {
                this._model = value;
                NotifyOfPropertyChange(() => this.Model);
            }
        }

        /// <summary>
        /// The user preference
        /// </summary>
        public Preference UserPreference { get; }

        /// <summary>
        /// Gets the window title.
        /// </summary>
        /// <value>The window title.</value>

        public string WindowTitle
        {
            get
            {
                var fp = "New Project" + "*";
                if (!string.IsNullOrWhiteSpace(this.FilePath))
                {
                    fp = Path.GetFileNameWithoutExtension(this.FilePath);
                }

                return $"{PathFolderHelper.ProgramName} {PathFolderHelper.GetProgramVersion()} - {fp}";
            }
        }

        /// <summary>
        /// Aborts the package creation.
        /// </summary>
        public void AbortPackageCreation()
        {
            if (this.ActiveBackgroungWorker != null)
            {
                this.ActiveBackgroungWorker.CancelAsync();

                if (this.exeProcess != null)
                {
                    this.exeProcess.Kill();
                }
            }

            this._abortPackageFlag = true;
        }

        /// <summary>
        /// Creates the new project.
        /// </summary>
        public void CreateNewProject()
        {
            MessageBoxResult rslt = MessageBox.Show("Save current project ?", "New Project", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (rslt == MessageBoxResult.Cancel)
            {
                return;
            }

            if (rslt == MessageBoxResult.Yes)
            {
                Save();
            }

            this.Model = new AutoSquirrelModel();
        }

        /// <summary>
        /// Opens the project.
        /// </summary>
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
                {
                    ofd.InitialDirectory = iniDir;
                }

                System.Windows.Forms.DialogResult o = ofd.ShowDialog();

                if (o != System.Windows.Forms.DialogResult.OK || !File.Exists(ofd.FileName))
                {
                    return;
                }

                OpenProject(ofd.FileName);

                //Save last folder path
            }
            catch (Exception)
            {
                MessageBox.Show("Loading File Error, file no more supported", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
            }
        }

        /// <summary>
        /// Opens the project.
        /// </summary>
        /// <param name="filepath">The filepath.</param>
        public void OpenProject(string filepath)
        {
            try
            {
                if (string.IsNullOrEmpty(filepath) || !File.Exists(filepath))
                {
                    MessageBox.Show("This file doesn't exist : " + filepath, "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
                    return;
                }

                this.FilePath = filepath;

                AutoSquirrelModel m = FileUtility.Deserialize<AutoSquirrelModel>(filepath);

                if (m == null)
                {
                    return;
                }

                this.Model = m;
                this.Model.PackageFiles = AutoSquirrelModel.OrderFileList(this.Model.PackageFiles);
                this.Model.RefreshPackageVersion();
                AddLastProject(filepath);
                NotifyOfPropertyChange(() => this.WindowTitle);
            }
            catch (Exception)
            {
                MessageBox.Show("Loading File Error, file no more supported", "Error", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.None);
            }
        }

        /// <summary>
        /// Publishes the package.
        /// </summary>
        /// <exception cref="Exception">
        /// Package Details are invalid or incomplete ! or Selected connection details are not valid !
        /// </exception>
        public void PublishPackage()
        {
            try
            {
                if (this.ActiveBackgroungWorker?.IsBusy == true)
                {
                    Trace.TraceError("You shouldn't be here !");
                    return;
                }

                this.Model.UploadQueue.Clear();
                this.Model.RefreshPackageVersion();

                Trace.WriteLine("START PUBLISHING ! : " + this.Model.Title);

                // 1) Check validity
                if (!this.Model.IsValid)
                {
                    throw new Exception("Package Details are invalid or incomplete !");
                }

                if (this.Model.SelectedConnection == null || !this.Model.SelectedConnection.IsValid)
                {
                    throw new Exception("Selected connection details are not valid !");
                }

                Trace.WriteLine("DATA VALIDATE - OK ! ");

                Save();

                // I proceed only if i created the project .asproj file and directory I need existing
                // directory to create the packages.

                if (!this._isSaved)
                {
                    return;
                }

                this.IsBusy = true;

                this.ActiveBackgroungWorker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

                this.ActiveBackgroungWorker.DoWork += this.ActiveBackgroungWorker_DoWork;
                this.ActiveBackgroungWorker.RunWorkerCompleted += this.PackageCreationCompleted;
                this.ActiveBackgroungWorker.ProgressChanged += this.ActiveBackgroungWorker_ProgressChanged;

                this.ActiveBackgroungWorker.RunWorkerAsync(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error on publishing", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 1) Check field validity
        /// 2) Create Nuget package
        /// 3) Squirrel relasify
        /// 4) Publish to amazon the updated file ( to get the update file , search the timedate &gt;
        ///    of building time ) /// - Possibly in async way..
        /// - Must be callable from command line, so i can optionally start this process from at the
        ///   end of visual studio release build
        /// </summary>
        public void PublishPackageComplete()
        {
            this._publishMode = 0;
            PublishPackage();
        }

        /// <summary>
        /// Publishes the package only update.
        /// </summary>
        public void PublishPackageOnlyUpdate()
        {
            this._publishMode = 1;
            PublishPackage();
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
        public void Save()
        {
            if (this.FilePath.Contains(".asproj"))
            {
                this.FilePath = Path.GetDirectoryName(this.FilePath);
            }
            if (string.IsNullOrWhiteSpace(this.FilePath))
            {
                SaveAs();
                return;
            }

            this.Model.NupkgOutputPath = this.FilePath + Path.DirectorySeparatorChar + this.Model.AppId + "_files" + PathFolderHelper.PackageDirectory;
            this.Model.SquirrelOutputPath = this.FilePath + Path.DirectorySeparatorChar + this.Model.AppId + "_files" + PathFolderHelper.ReleasesDirectory;

            if (!Directory.Exists(this.Model.NupkgOutputPath))
            {
                Directory.CreateDirectory(this.Model.NupkgOutputPath);
            }

            if (!Directory.Exists(this.Model.SquirrelOutputPath))
            {
                Directory.CreateDirectory(this.Model.SquirrelOutputPath);
            }

            var asProj = Path.Combine(this.FilePath, $"{this.Model.AppId}.asproj");
            FileUtility.SerializeToFile(asProj, this.Model);

            Trace.WriteLine("FILE SAVED ! : " + this.FilePath);

            this._isSaved = true;

            AddLastProject(asProj);

            NotifyOfPropertyChange(() => this.WindowTitle);
        }

        /// <summary>
        /// Saves as.
        /// </summary>
        public void SaveAs()
        {
            var previousFilePath = this.FilePath;

            try
            {
                var saveFileDialog = new System.Windows.Forms.FolderBrowserDialog
                {
                    SelectedPath = PathFolderHelper.GetMyDirectory(MyDirectory.Project),
                    ShowNewFolderButton = true
                };

                if (saveFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

                this.FilePath = saveFileDialog.SelectedPath;

                Save();
            }
            catch (Exception)
            {
                MessageBox.Show("Error on saving");

                this.FilePath = previousFilePath;
            }
        }

        internal string CreateNugetPackage(AutoSquirrelModel model)
        {
            var metadata = new ManifestMetadata()
            {
                Id = model.AppId,
                Authors = model.Authors,
                Version = model.Version,
                Description = model.Description,
                Title = model.Title,
            };

            var builder = new PackageBuilder();
            builder.Populate(metadata);

            //As Squirrel convention i put everything in lib/net45 folder

            const string directoryBase = "/lib/net45";

            var files = new List<ManifestFile>();

            foreach (ItemLink node in model.PackageFiles)
            {
                AddFileToPackage(directoryBase, node, files);
            }

            builder.PopulateFiles("", files.ToArray());

            var nugetPath = model.NupkgOutputPath + model.AppId + "." + model.Version + ".nupkg";

            using (FileStream stream = File.Open(nugetPath, FileMode.OpenOrCreate))
            {
                builder.Save(stream);
            }

            return nugetPath;
        }

        private static void AddFileToPackage(string directoryBase, ItemLink node, List<ManifestFile> files)
        {
            // Don't add manifest if is directory

            if (node.IsDirectory)
            {
                directoryBase += "/" + node.Filename;

                foreach (ItemLink subNode in node.Children)
                {
                    AddFileToPackage(directoryBase, subNode, files);
                }
            }
            else
            {
                var manifest = new ManifestFile()
                {
                    Source = node.SourceFilepath
                };

                manifest.Target = directoryBase + "/" + node.Filename;

                files.Add(manifest);
            }
        }

        private void ActiveBackgroungWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                this.ActiveBackgroungWorker.ReportProgress(20, "NUGET PACKAGE CREATING");

                // Create Nuget Package from package treeview.
                var nugetPackagePath = CreateNugetPackage(this.Model);
                Trace.WriteLine("CREATED NUGET PACKAGE to : " + this.Model.NupkgOutputPath);

                if (this.ActiveBackgroungWorker.CancellationPending)
                {
                    return;
                }

                this.ActiveBackgroungWorker.ReportProgress(40, "SQUIRREL PACKAGE CREATING");

                // Releasify
                SquirrelReleasify(nugetPackagePath, this.Model.SquirrelOutputPath);
                Trace.WriteLine("CREATED SQUIRREL PACKAGE to : " + this.Model.SquirrelOutputPath);
            }
            catch (Exception ex)
            {
                e.Result = ex;
            }
        }

        private void ActiveBackgroungWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            //todo : Update busy indicator information.
            var message = e.UserState as string;
            if (message == null)
            {
                return;
            }

            this.CurrentPackageCreationStage = message;
        }

        private void AddLastProject(string filePath)
        {
            foreach (var fp in this.UserPreference.LastOpenedProject.Where(p => p.ToLower() == filePath.ToLower()).ToList())
            {
                this.UserPreference.LastOpenedProject.Remove(fp);
            }

            this.UserPreference.LastOpenedProject.Add(filePath);

            PathFolderHelper.SavePreference(this.UserPreference);
        }

        /// <summary>
        /// Called on package created. Start the upload.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackageCreationCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.IsBusy = false;

            this.CurrentPackageCreationStage = string.Empty;

            this.ActiveBackgroungWorker.Dispose();

            this.ActiveBackgroungWorker = null;

            if (this._abortPackageFlag)
            {
                if (this.Model.UploadQueue != null)
                {
                    this.Model.UploadQueue.Clear();
                }

                this._abortPackageFlag = false;

                return;
            }

            if (e.Result is Exception ex)
            {
                MessageBox.Show(ex.Message, "Package creation error", MessageBoxButton.OK, MessageBoxImage.Error);

                //todo : Manage generated error
                return;
            }

            if (e.Cancelled)
            {
                return;
            }

            // Start uploading generated files.
            this.Model.BeginUpdatedFiles(this._publishMode);
        }

        private void SquirrelReleasify(string nugetPackagePath, string squirrelOutputPath)
        {
            /*
            https://github.com/Squirrel/Squirrel.Windows/blob/c86d3d0f19418d9f31d244f9c1d96d25a9c0dfb6/src/Update/Program.cs
                    "Options:",
                    { "h|?|help", "Display Help and exit", _ => {} },
                    { "r=|releaseDir=", "Path to a release directory to use with releasify", v => releaseDir = v},
                    { "p=|packagesDir=", "Path to the NuGet Packages directory for C# apps", v => packagesDir = v},
                    { "bootstrapperExe=", "Path to the Setup.exe to use as a template", v => bootstrapperExe = v},
                    { "g=|loadingGif=", "Path to an animated GIF to be displayed during installation", v => backgroundGif = v},
                    { "i=|icon", "Path to an ICO file that will be used for icon shortcuts", v => icon = v},
                    { "setupIcon=", "Path to an ICO file that will be used for the Setup executable's icon", v => setupIcon = v},
                    { "n=|signWithParams=", "Sign the installer via SignTool.exe with the parameters given", v => signingParameters = v},
                    { "s|silent", "Silent install", _ => silentInstall = true},
                    { "b=|baseUrl=", "Provides a base URL to prefix the RELEASES file packages with", v => baseUrl = v, true},
                    { "a=|process-start-args=", "Arguments that will be used when starting executable", v => processStartArgs = v, true},
                    { "l=|shortcut-locations=", "Comma-separated string of shortcut locations, e.g. 'Desktop,StartMenu'", v => shortcutArgs = v},
                    { "no-msi", "Don't generate an MSI package", v => noMsi = true},
            */
            var cmd = $@" -releasify {nugetPackagePath} -releaseDir {squirrelOutputPath} -l 'Desktop'";

            if (File.Exists(this.Model.IconFilepath))
            {
                cmd += @" -i " + this.Model.IconFilepath;
                cmd += @" -setupIcon " + this.Model.IconFilepath;
            }

            var startInfo = new ProcessStartInfo()
            {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = @"tools\Squirrel-Windows.exe",
                Arguments = cmd
            };

            using (this.exeProcess = Process.Start(startInfo))
            {
                this.exeProcess.WaitForExit();
            }
        }
    }
}