namespace AutoSquirrel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using Amazon;
    using FluentValidation;
    using FluentValidation.Results;

    /// <summary>
    /// This class contains all information about WebConncetion uploading. Information for user :
    /// Credentials are stored in clear format.
    /// </summary>
    [DataContract]
    public class AmazonS3Connection : WebConnectionBase
    {
        private string _accessKey;

        private List<string> _availableRegionList;

        //http://docs.aws.amazon.com/awscloudtrail/latest/userguide/cloudtrail-s3-bucket-naming-requirements.html
        private string _bucketName;

        private string _regionName;

        private string _secretAccessKey;

        public AmazonS3Connection()
        {
            ConnectionName = "Amazon S3";
        }

        /// <summary>
        /// Gets or sets the access key.
        /// </summary>
        /// <value>The access key.</value>
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

        /// <summary>
        /// Gets the available region list.
        /// </summary>
        /// <value>The available region list.</value>
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

        /// <summary>
        /// Gets or sets the name of the bucket.
        /// </summary>
        /// <value>The name of the bucket.</value>
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

        /// <summary>
        /// Gets or sets the name of the region.
        /// </summary>
        /// <value>The name of the region.</value>
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

        /// <summary>
        /// Gets or sets the secret access key.
        /// </summary>
        /// <value>The secret access key.</value>
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

        /// <summary>
        /// Gets the setup download URL.
        /// </summary>
        /// <value>The setup download URL.</value>
        public string SetupDownloadUrl
        {
            get
            {
                if (string.IsNullOrWhiteSpace(BucketName) || string.IsNullOrWhiteSpace(RegionName))
                    return "Missing Parameter";

                return "https://s3-" + GetRegion().SystemName + ".amazonaws.com/" + BucketName.ToLower() + "/Setup.exe";
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

        internal RegionEndpoint GetRegion() =>
            RegionEndpoint.EnumerableAllRegions.FirstOrDefault(r => r.DisplayName == RegionName);

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
}