namespace AutoSquirrel
{
    using System;

    /// <summary>
    /// Upload Complete EventArgs
    /// </summary>
    /// <seealso cref="System.EventArgs"/>
    public class UploadCompleteEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadCompleteEventArgs"/> class.
        /// </summary>
        /// <param name="sfu">The sfu.</param>
        public UploadCompleteEventArgs(SingleFileUpload sfu)
        {
            FileUploaded = sfu;
        }

        /// <summary>
        /// Gets the file uploaded.
        /// </summary>
        /// <value>The file uploaded.</value>
        public SingleFileUpload FileUploaded { get; internal set; }
    }
}