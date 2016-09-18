namespace AutoSquirrel
{
    using System;
    using System.Runtime.Serialization;
    using FluentValidation;
    using FluentValidation.Results;

    /// <summary>
    /// This class contains all information about WebConncetion uploading. Information for user :
    /// Credentials are stored in clear format.
    /// </summary>
    [DataContract]
    public class FileSystemConnection : WebConnectionBase
    {
        private string _fileSystemPath;

        public FileSystemConnection()
        {
            ConnectionName = "File System";
        }

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
}