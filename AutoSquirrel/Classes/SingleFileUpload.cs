using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Threading;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Caliburn.Micro;

namespace AutoSquirrel
{
    /// <summary>
    /// Used in Upload queue list. I don't need serialization for this class.
    /// </summary>
    [DataContract]
    public class SingleFileUpload : PropertyChangedBase
    {
        private string _connection;
        private string _filename;
        private string _fileSize;
        private double _progressPercentage;
        private FileUploadStatus _uploadStatus;
        private TransferUtility fileTransferUtility;

        /// <summary>
        /// Occurs when [on upload completed].
        /// </summary>
        public event EventHandler<UploadCompleteEventArgs> OnUploadCompleted;

        /// <summary>
        /// Gets the connection.
        /// </summary>
        /// <value>The connection.</value>
        public WebConnectionBase Connection { get; internal set; }

        /// <summary>
        /// Gets or sets the name of the connection.
        /// </summary>
        /// <value>The name of the connection.</value>
        [DataMember]
        public string ConnectionName
        {
            get => _connection;

            set
            {
                _connection = value;
                NotifyOfPropertyChange(() => ConnectionName);
            }
        }

        /// <summary>
        /// Gets or sets the filename.
        /// </summary>
        /// <value>The filename.</value>
        [DataMember]
        public string Filename
        {
            get => _filename;

            set
            {
                _filename = value;
                NotifyOfPropertyChange(() => Filename);
            }
        }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        /// <value>The size of the file.</value>
        [DataMember]
        public string FileSize
        {
            get => _fileSize;

            set
            {
                _fileSize = value;
                NotifyOfPropertyChange(() => FileSize);
            }
        }

        /// <summary>
        /// Gets the formatted status.
        /// </summary>
        /// <value>The formatted status.</value>
        public string FormattedStatus => UploadStatus.ToString();

        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <value>The full path.</value>
        public string FullPath { get; internal set; }

        /// <summary>
        /// Gets or sets the progress percentage.
        /// </summary>
        /// <value>The progress percentage.</value>
        [DataMember]
        public double ProgressPercentage
        {
            get => _progressPercentage;

            set
            {
                _progressPercentage = value;
                NotifyOfPropertyChange(() => ProgressPercentage);
            }
        }

        /// <summary>
        /// Gets or sets the upload status.
        /// </summary>
        /// <value>The upload status.</value>
        public FileUploadStatus UploadStatus
        {
            get => _uploadStatus;

            set
            {
                _uploadStatus = value;
                NotifyOfPropertyChange(() => UploadStatus);
                NotifyOfPropertyChange(() => FormattedStatus);
            }
        }

        internal void StartUpload()
        {
            if (Connection is AmazonS3Connection amazonCon) {
                if (!CheckInternetConnection.IsConnectedToInternet()) {
                    throw new Exception("Internet Connection not available");
                }

                var amazonClient = new AmazonS3Client(amazonCon.AccessKey, amazonCon.SecretAccessKey, amazonCon.GetRegion());

                fileTransferUtility = new TransferUtility(amazonClient);

                if (!(AmazonS3Util.DoesS3BucketExist(amazonClient, amazonCon.BucketName))) {
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
            } else if (Connection is FileSystemConnection fileCon) {
                uploadRequest_UploadPartProgressEvent(this, new UploadProgressArgs(100, 100, 100));
            }
        }

        private static void CreateABucket(IAmazonS3 client, string bucketName)
        {
            var putRequest1 = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true
            };

            var response1 = client.PutBucket(putRequest1);

            Trace.WriteLine("Creating a bucket " + bucketName);
        }

        private void RequesteUploadComplete(UploadCompleteEventArgs uploadEvent)
        {
            UploadStatus = FileUploadStatus.Completed;
            ProgressPercentage = 100;

            OnUploadCompleted?.Invoke(null, uploadEvent);
        }

        private void uploadRequest_UploadPartProgressEvent(
          object sender, UploadProgressArgs e)
        {
            ProgressPercentage = e.PercentDone;

            if (e.PercentDone == 100) {
                if (Application.Current.Dispatcher.CheckAccess()) {
                    RequesteUploadComplete(new UploadCompleteEventArgs(this));
                } else {
                    Application.Current.Dispatcher.BeginInvoke(
                      DispatcherPriority.Background,
                      new System.Action(() => RequesteUploadComplete(new UploadCompleteEventArgs(this))));
                }
            }
        }
    }
}
