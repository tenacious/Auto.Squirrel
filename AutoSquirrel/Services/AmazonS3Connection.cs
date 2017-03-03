using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Amazon;
using AutoSquirrel;
using FluentValidation;
using FluentValidation.Results;

namespace AutoSquirrel
{
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

        /// <summary>
        /// Initializes a new instance of the <see cref="AmazonS3Connection"/> class.
        /// </summary>
        public AmazonS3Connection() => this.ConnectionName = "Amazon S3";

        /// <summary>
        /// Gets or sets the access key.
        /// </summary>
        /// <value>The access key.</value>
        [DataMember]
        public string AccessKey
        {
            get => this._accessKey;

            set
            {
                this._accessKey = value;
                NotifyOfPropertyChange(() => this.AccessKey);
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
                if (this._availableRegionList == null)
                {
                    this._availableRegionList = new List<string>();

                    foreach (RegionEndpoint r in RegionEndpoint.EnumerableAllRegions)
                    {
                        this._availableRegionList.Add(r.DisplayName);
                    }
                }

                return this._availableRegionList;
            }
        }

        /// <summary>
        /// Gets or sets the name of the bucket.
        /// </summary>
        /// <value>The name of the bucket.</value>
        [DataMember]
        public string BucketName
        {
            get => this._bucketName;

            set
            {
                this._bucketName = value;
                if (this._bucketName != null)
                {
                    this._bucketName = this._bucketName.ToLower().Replace(" ", string.Empty);
                }

                NotifyOfPropertyChange(() => this.BucketName);
                NotifyOfPropertyChange(() => this.SetupDownloadUrl);
            }
        }

        /// <summary>
        /// Gets or sets the name of the region.
        /// </summary>
        /// <value>The name of the region.</value>
        [DataMember]
        public string RegionName
        {
            get => this._regionName;

            set
            {
                this._regionName = value;
                NotifyOfPropertyChange(() => this.RegionName);
                NotifyOfPropertyChange(() => this.SetupDownloadUrl);
            }
        }

        /// <summary>
        /// Gets or sets the secret access key.
        /// </summary>
        /// <value>The secret access key.</value>
        [DataMember]
        public string SecretAccessKey
        {
            get => this._secretAccessKey;

            set
            {
                this._secretAccessKey = value;
                NotifyOfPropertyChange(() => this.SecretAccessKey);
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
                if (string.IsNullOrWhiteSpace(this.BucketName) || string.IsNullOrWhiteSpace(this.RegionName))
                {
                    return "Missing Parameter";
                }

                return "https://s3-" + GetRegion().SystemName + ".amazonaws.com/" + this.BucketName.ToLower() + "/Setup.exe";
            }
        }

        /// <summary>
        /// Validates this instance.
        /// </summary>
        /// <returns></returns>
        public override ValidationResult Validate()
        {
            ValidationResult commonValid = new Validator().Validate(this);
            if (!commonValid.IsValid)
            {
                return commonValid;
            }

            return base.Validate();
        }

        internal RegionEndpoint GetRegion() =>
            RegionEndpoint.EnumerableAllRegions.FirstOrDefault(r => r.DisplayName == this.RegionName);

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
                {
                    return false;
                }

                return true;
            }
        }
    }
}