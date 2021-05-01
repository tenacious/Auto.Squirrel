using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media;
using Caliburn.Micro;
using static AutoSquirrel.IconHelper;

namespace AutoSquirrel
{
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
            get => _children;

            set
            {
                _children = value;
                NotifyOfPropertyChange(() => Children);
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

                    if (IsDirectory && IsExpanded) {
                        icon = IconHelper.GetFolderIcon(IconSize.Large, FolderType.Open);
                    } else if (IsDirectory && !IsExpanded) {
                        icon = IconHelper.GetFolderIcon(IconSize.Large, FolderType.Closed);
                    } else {
                        if (File.Exists(SourceFilepath)) {
                            icon = Icon.ExtractAssociatedIcon(SourceFilepath);
                        } else {
                            return IconHelper.FindIconForFilename(Path.GetFileName(SourceFilepath), true);
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
                if (!string.IsNullOrWhiteSpace(OutputFilename)) {
                    return OutputFilename;
                }

                if (!string.IsNullOrWhiteSpace(SourceFilepath)) {
                    return Path.GetFileName(SourceFilepath);
                }

                return "no_namefile";
            }

            set
            {
                OutputFilename = value;
                NotifyOfPropertyChange(() => Filename);
            }
        }

        /// <summary>
        /// Returns true if this object's Children have not yet been populated.
        /// </summary>
        public bool HasDummyChild => Children.Count == 1 && Children[0] == DummyChild;

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
            get => _isExpanded;

            set
            {
                if (value != _isExpanded) {
                    _isExpanded = value;
                    NotifyOfPropertyChange(() => IsExpanded);
                    NotifyOfPropertyChange(() => FileIcon);
                }

                // Lazy load the child items, if necessary.
                if (HasDummyChild) {
                    Children.Remove(DummyChild);
                    LoadChildren();
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
            get => _isSelected;

            set
            {
                if (value != _isSelected) {
                    _isSelected = value;
                    NotifyOfPropertyChange(() => IsSelected);
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
            get => sourceFilepath;

            set
            {
                sourceFilepath = value;
                NotifyOfPropertyChange(() => SourceFilepath);
                NotifyOfPropertyChange(() => Filename);
                try {
                    var fa = File.GetAttributes(value);
                    if ((fa & FileAttributes.Directory) != 0) {
                        SetDirectoryInfo(value);
                        return;
                    }

                    var fileInfo = new FileInfo(value);
                    LastEdit = fileInfo.LastWriteTime.ToString();
                    FileDimension = fileInfo.Length;
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
            foreach (var node in root) {
                var p = FindParent(this, node);
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

                foreach (var child in node.Children) {
                    var p = FindParent(link, child);
                    if (p != null) {
                        return p;
                    }
                }
            }

            return null;
        }

        private static string GetDirectoryName(string relativeOutputPath)
        {
            var directories = relativeOutputPath.Split(new List<char> { Path.DirectorySeparatorChar }.ToArray(), StringSplitOptions.RemoveEmptyEntries);

            return directories.LastOrDefault();
        }

        private void SetDirectoryInfo(string folderPath)
        {
            var dirInfo = new DirectoryInfo(folderPath);
            LastEdit = dirInfo.LastWriteTime.ToString();
            FileDimension = dirInfo.EnumerateFiles("*.*", SearchOption.AllDirectories).Sum(fi => fi.Length);
        }
    }
}
