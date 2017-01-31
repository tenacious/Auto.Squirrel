namespace AutoSquirrel
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// </summary>
    /// <seealso cref="AutoSquirrel.PropertyChangedBaseValidable"/>
    public class WebConnectionBase : PropertyChangedBaseValidable
    {
        private string _connectionName;

        /// <summary>
        /// Gets or sets the name of the connection.
        /// </summary>
        /// <value>The name of the connection.</value>
        [DataMember]
        public string ConnectionName
        {
            get
            {
                return this._connectionName;
            }

            set
            {
                this._connectionName = value;
                NotifyOfPropertyChange(() => this.ConnectionName);
            }
        }
    }
}