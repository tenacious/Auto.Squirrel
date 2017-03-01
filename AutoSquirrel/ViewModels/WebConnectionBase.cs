namespace AutoSquirrel
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Web Connection Base
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
            get => this._connectionName;

            set
            {
                this._connectionName = value;
                NotifyOfPropertyChange(() => this.ConnectionName);
            }
        }
    }
}