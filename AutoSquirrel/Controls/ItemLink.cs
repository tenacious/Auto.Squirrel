namespace AutoSquirrel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Windows.Media;
    using Caliburn.Micro;
    using static IconHelper;

    /// <summary>
    /// Item Link
    /// </summary>
    /// <seealso cref="Caliburn.Micro.PropertyChangedBase"/>
    [DataContract]
    public class ItemLink : PropertyChangedBase
    {
        private static readonly ItemLink DummyChild = new ItemLink();

        [DataMember]
        private ObservableCollection<ItemLink> _children = new ObservableCollection<ItemLink>();

        private bool _isSelected;
        private string sourceFilepath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemLink"/> class.
        /// </summary>
        public ItemLink()
        {
        }

        /// <summary>
        /// Returns the logical child items of this object.
        /// </summary>
        public ObservableCollection<ItemLink> Children
        {
            get => this._children;

            set
            {
                this._children = value;
                NotifyOfPropertyChange(() => this.Children);
            }
        }

        /// <summary>
        /// Gets the file dimension.
        /// </summary>
        /// <value>The file dimension.</value>
        [DataMember]
        public double FileDimension { get; internal set; }

        /// <summary>
        /// Gets the file icon.
        /// </summary>
        /// <value>The file icon.</value>
        public ImageSource FileIcon
        {
            get
            {
                try {
                    Icon icon = null;

                    if (this.IsDirectory && this.IsExpanded) {
                        icon = IconHelper.GetFolderIcon(IconSize.Large, FolderType.Open);
                    } else if (this.IsDirectory && !this.IsExpanded) {
                        icon = IconHelper.GetFolderIcon(IconSize.Large, FolderType.Closed);
                    } else {
                        if (File.Exists(this.SourceFilepath)) {
                            icon = Icon.ExtractAssociatedIcon(this.SourceFilepath);
                        } else {
                            return IconHelper.FindIconForFilename(Path.GetFileName(this.SourceFilepath), true);
                        }
                    }
                    if (icon == null) {
                        return null;
                    }

                    return icon.ToImageSource();
                } catch {

                    //TODO - Get default icon
                    return null;
                }
            }
        }

        //private string _filename;
        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        /// <value>The filename.</value>
        [DataMember]
        public string Filename
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.OutputFilename)) {
                    return this.OutputFilename;
                }

                if (!string.IsNullOrWhiteSpace(this.SourceFilepath)) {
                    return Path.GetFileName(this.SourceFilepath);
                }

                return "no_namefile";
            }

            set
            {
                this.OutputFilename = value;
                NotifyOfPropertyChange(() => this.Filename);
            }
        }

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild => this.Children.Count == 1 && this.Children[0] == DummyChild;

        /// <summary>
        /// Gets or sets a value indicating whether this instance is directory.
        /// </summary>
        /// <value><c>true</c> if this instance is directory; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool IsDirectory { get; set; }

        /// <summary>
        /// Gets/sets whether the TreeViewItem associated with this object is expanded.
        /// </summary>
        [DataMember]
        public bool IsExpanded
        {
            get => this._isExpanded;

            set
            {
                if (value != this._isExpanded) {
                    this._isExpanded = value;
                    NotifyOfPropertyChange(() => this.IsExpanded);
                    NotifyOfPropertyChange(() => this.FileIcon);
                }

                // Lazy load the child items, if necessary.
                if (this.HasDummyChild) {
                    this.Children.Remove(DummyChild);
                    this.LoadChildren();
                }
            }
        }

        /// <summary>
        /// Fixed folder. Can't remove or move.
        /// </summary>
        [DataMember]
        public bool IsRootBase { get; internal set; }

        /// <summary>
        /// Gets/sets whether the TreeViewItem associated with this object is selected.
        /// </summary>
        [DataMember]
        public bool IsSelected
        {
            get => this._isSelected;

            set
            {
                if (value != this._isSelected) {
                    this._isSelected = value;
                    NotifyOfPropertyChange(() => this.IsSelected);
                }
            }
        }

        /// <summary>
        /// Gets the last edit.
        /// </summary>
        /// <value>The last edit.</value>
        [DataMember]
        public string LastEdit { get; internal set; }

        /// <summary>
        /// Gets the output filename.
        /// </summary>
        /// <value>The output filename.</value>
        [DataMember]
        public string OutputFilename { get; internal set; }

        /// <summary>
        /// Filepath of linked source file. Absolute ?
        /// </summary>
        [DataMember]
        public string SourceFilepath
        {
            get => this.sourceFilepath;

            set
            {
                this.sourceFilepath = value;
                NotifyOfPropertyChange(() => this.SourceFilepath);
                NotifyOfPropertyChange(() => this.Filename);
                try {
                    FileAttributes fa = File.GetAttributes(value);
                    if ((fa & FileAttributes.Directory) != 0) {
                        this.SetDirectoryInfo(value);
                        return;
                    }

                    var fileInfo = new FileInfo(value);
                    this.LastEdit = fileInfo.LastWriteTime.ToString();
                    this.FileDimension = fileInfo.Length;
                } catch {
                }
            }
        }

        private bool _isExpanded { get; set; }

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <returns></returns>
        public ItemLink GetParent(ObservableCollection<ItemLink> root)
        {
            foreach (ItemLink node in root) {
                ItemLink p = FindParent(this, node);
                if (p != null) {
                    return p;
                }
            }

            return null;
        }

        /// <summary>
        /// Invoked when the child items need to be loaded on demand. Subclasses can override this to
        /// populate the Children collection.
        /// </summary>
        protected virtual void LoadChildren()
        {
        }

        private static ItemLink FindParent(ItemLink link, ItemLink node)
        {
            if (node.Children != null) {
                if (node.Children.Contains(link)) {
                    return node;
                }

                foreach (ItemLink child in node.Children) {
                    ItemLink p = FindParent(link, child);
                    if (p != null) {
                        return p;
                    }
                }
            }

            return null;
        }

        private static string GetDirectoryName(string relativeOutputPath)
        {
            string[] directories = relativeOutputPath.Split(new List<char> { Path.DirectorySeparatorChar }.ToArray(), StringSplitOptions.RemoveEmptyEntries);

            return directories.LastOrDefault();
        }

        private void SetDirectoryInfo(string folderPath)
        {
            var dirInfo = new DirectoryInfo(folderPath);
            this.LastEdit = dirInfo.LastWriteTime.ToString();
            this.FileDimension = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
    }
}