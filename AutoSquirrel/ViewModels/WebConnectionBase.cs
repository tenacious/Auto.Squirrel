using System;
using System.Runtime.Serialization;

namespace AutoSquirrel
{
    /// <summary>
    /// Web Connection Base
    /// </summary>
    /// <seealso cref="AutoSquirrel.PropertyChangedBaseValidable"/>
    public abstract class WebConnectionBase : PropertyChangedBaseValidable
    {
        private string _connectionName;

        /// <summary>
        /// Gets or sets the name of the connection.
        /// </summary>
        /// <value>The name of the connection.</value>
        [DataMember]
        public string ConnectionName
        {
            get => _connectionName;

            set
            {
                _connectionName = value;
                NotifyOfPropertyChange(() => ConnectionName);
            }
        }
    }
}
