using System;
using System.Runtime.Serialization;
using FluentValidation;
using FluentValidation.Results;

namespace AutoSquirrel
{
    /// <summary>
    /// This class contains all information about WebConncetion uploading. Information for user :
    /// Credentials are stored in clear format.
    /// </summary>
    [DataContract]
    public class FileSystemConnection : WebConnectionBase
    {
        private string _fileSystemPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemConnection"/> class.
        /// </summary>
        public FileSystemConnection() => ConnectionName = "File System";

        /// <summary>
        /// Gets or sets the file system path.
        /// </summary>
        /// <value>The file system path.</value>
        [DataMember]
        public string FileSystemPath
        {
            get => _fileSystemPath;

            set
            {
                _fileSystemPath = value;

                NotifyOfPropertyChange(() => FileSystemPath);
                NotifyOfPropertyChange(() => SetupDownloadUrl);
            }
        }

        /// <summary>
        /// Gets the setup download URL.
        /// </summary>
        /// <value>The setup download URL.</value>
        public string SetupDownloadUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FileSystemPath)) {
                    return "Missing Parameter";
                }

                return FileSystemPath + "\\Setup.exe";
            }
        }

        /// <summary>
        /// Prima controllo correttezza del pattern poi controllo questo.
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

        private class Validator : AbstractValidator<FileSystemConnection>
        {
            public Validator() => RuleFor(c => c.FileSystemPath).NotEmpty();
        }
    }
}
