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
        /// <summary>
        /// The model
        /// </summary>
        public AutoSquirrelModel _model;

        /// <summary>
        /// The user preference
        /// </summary>
        public Preference UserPreference;

        internal BackgroundWorker ActiveBackgroungWorker;

        private ICommand _abortPackageCreationCmd;

        private bool _abortPackageFlag;

        private string _currentPackageCreationStage;

        /// <summary>
        /// Show/Hide Busy indicatory
        /// </summary>
        private bool _isBusy;

        private bool _isSaved;

        private int _publishMode;

        private Process exeProcess;

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

        /// <summary>
        /// Gets the abort package creation command.
        /// </summary>
        /// <value>The abort package creation command.</value>
        public ICommand AbortPackageCreationCmd
        {
            get
            {
                return _abortPackageCreationCmd ??
                       (_abortPackageCreationCmd = new DelegateCommand(AbortPackageCreation));
            }
        }

        /// <summary>
        /// Gets or sets the current package creation stage.
        /// </summary>
        /// <value>The current package creation stage.</value>
        public string CurrentPackageCreationStage
        {
            get
            {
                return _currentPackageCreationStage;
            }

            set
            {
                _currentPackageCreationStage = value;
                NotifyOfPropertyChange(() => CurrentPackageCreationStage);
            }
        }

        /// <summary>
        /// Gets or sets the file path.
        /// </summary>
        /// <value>The file path.</value>
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

        /// <summary>
        /// Gets or sets a value indicating whether this instance is busy.
        /// </summary>
        /// <value><c>true</c> if this instance is busy; otherwise, <c>false</c>.</value>
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
        /// Gets or sets the model.
        /// </summary>
        /// <value>The model.</value>
        public AutoSquirrelModel Model
        {
            get
            {
                return _model;
            }

            set
            {
                _model = value;
                NotifyOfPropertyChange(() => Model);
            }
        }

        //// M E T H O D S

        /// <summary>
        /// Gets the window title.
        /// </summary>
        /// <value>The window title.</value>

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
        /// Aborts the package creation.
        /// </summary>
        public void AbortPackageCreation()
        {
            if (ActiveBackgroungWorker != null)
            {
                ActiveBackgroungWorker.CancelAsync();

                if (exeProcess != null)
                    exeProcess.Kill();
            }

            _abortPackageFlag = true;
        }

        /// <summary>
        /// Creates the new project.
        /// </summary>
        public void CreateNewProject()
        {
            var rslt = MessageBox.Show("Save current project ?", "New Project", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

            if (rslt == MessageBoxResult.Cancel) return;

            if (rslt == MessageBoxResult.Yes)
                Save();

            Model = new AutoSquirrelModel();
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
                    ofd.InitialDirectory = iniDir;

                var o = ofd.ShowDialog();

                if (o != System.Windows.Forms.DialogResult.OK || !File.Exists(ofd.FileName)) return;

                OpenProject(ofd.FileName);

                //Save last folder path
            }
            catch (Exception)
            {
                MessageBox.Show("Loading File Error, file no more supported", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.None);
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
            catch (Exception)
            {
                MessageBox.Show("Loading File Error, file no more supported", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error, MessageBoxResult.None);
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

                // I proceed only if i created the project .asproj file and directory I need existing
                // directory to create the packages.

                if (!_isSaved)
                    return;

                IsBusy = true;

                ActiveBackgroungWorker = new BackgroundWorker() { WorkerReportsProgress = true, WorkerSupportsCancellation = true };

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
            }
        }

        /// <summary>
        /// 1) Check field validity
        /// 2) Create Nuget package
        /// 3) Squirrel relasify
        /// 4) Publish to amazon the updated file ( to get the update file , search the timedate &gt;
        ///    of building time )
        ///
        /// - Possibly in async way..
        /// - Must be callable from command line, so i can optionally start this process from at the
        ///   end of visual studio release build
        /// </summary>
        public void PublishPackageComplete()
        {
            _publishMode = 0;
            PublishPackage();
        }

        /// <summary>
        /// Publishes the package only update.
        /// </summary>
        public void PublishPackageOnlyUpdate()
        {
            _publishMode = 1;
            PublishPackage();
        }

        /// <summary>
        /// Saves this instance.
        /// </summary>
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

        /// <summary>
        /// Saves as.
        /// </summary>
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

            PackageBuilder builder = new PackageBuilder();
            builder.Populate(metadata);

            //As Squirrel convention i put everything in lib/net45 folder

            var directoryBase = "/lib/net45";

            var files = new List<ManifestFile>();

            foreach (var node in model.PackageFiles)
            {
                AddFileToPackage(directoryBase, node, files);
            }

            builder.PopulateFiles("", files.ToArray());

            var nugetPath = model.NupkgOutputPath + Path.DirectorySeparatorChar + model.AppId + "." + model.Version + ".nupkg";

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

                foreach (var subNode in node.Children)
                {
                    AddFileToPackage(directoryBase, subNode, files);
                }
            }
            else
            {
                //if (File.Exists(node.SourceFilepath))
                {
                    var manifest = new ManifestFile();

                    manifest.Source = node.SourceFilepath;
                    {
                        var extension = Path.GetExtension(node.SourceFilepath);

                        //var filename = Path.GetFileNameWithoutExtension(node.Filename);
                        //manifest.Target = directoryBase + "/" + filename + "_ll_" + extension;
                        manifest.Target = directoryBase + "/" + node.Filename;
                    }

                    files.Add(manifest);
                }
            }
        }

        private void ActiveBackgroungWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                ActiveBackgroungWorker.ReportProgress(20, "NUGET PACKAGE CREATING");

                // Create Nuget Package from package treeview.
                var nugetPackagePath = CreateNugetPackage(Model);
                Trace.WriteLine("CREATED NUGET PACKAGE to : " + Model.NupkgOutputPath);

                if (ActiveBackgroungWorker.CancellationPending)
                    return;

                ActiveBackgroungWorker.ReportProgress(40, "SQUIRREL PACKAGE CREATING");

                // Releasify
                SquirrelReleasify(nugetPackagePath, Model.SquirrelOutputPath);
                Trace.WriteLine("CREATED SQUIRREL PACKAGE to : " + Model.SquirrelOutputPath);
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
            if (message == null) return;

            CurrentPackageCreationStage = message;
        }

        private void AddLastProject(string filePath)
        {
            var existing = UserPreference.LastOpenedProject.Where(p => p.ToLower() == filePath.ToLower()).ToList();

            foreach (var fp in existing)
                UserPreference.LastOpenedProject.Remove(fp);

            UserPreference.LastOpenedProject.Add(filePath);

            PathFolderHelper.SavePreference(UserPreference);
        }

        /// <summary>
        /// Called on package created. Start the upload.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PackageCreationCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsBusy = false;

            CurrentPackageCreationStage = string.Empty;

            ActiveBackgroungWorker.Dispose();

            ActiveBackgroungWorker = null;

            if (_abortPackageFlag)
            {
                if (Model.UploadQueue != null)
                    Model.UploadQueue.Clear();

                _abortPackageFlag = false;

                return;
            }

            var ex = e.Result as Exception;
            if (ex != null)
            {
                MessageBox.Show(ex.Message, "Package creation error", MessageBoxButton.OK, MessageBoxImage.Error);

                //todo : Manage generated error
                return;
            }

            if (e.Cancelled) return;

            // Start uploading generated files.
            Model.BeginUpdatedFiles(_publishMode);
        }

        private void SquirrelReleasify(string nugetPackagePath, string squirrelOutputPath)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;

            startInfo.FileName = @"tools\Squirrel.exe";

            var cmd = @" -releasify " + nugetPackagePath + " -releaseDir " + squirrelOutputPath;

            //if (File.Exists(Model.IconFilepath))
            //  cmd += @" -setupIcon " + Model.IconFilepath ;

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
            startInfo.Arguments = cmd;

            using (exeProcess = Process.Start(startInfo))
            {
                exeProcess.WaitForExit();
            }
        }
    }
}