using System.Collections.Generic;

namespace AutoSquirrel
{
    /// <summary>
    /// The connection discovery service.
    /// </summary>
    public interface IConnectionDiscoveryService
    {
        /// <summary>
        /// Gets the available connections.
        /// </summary>
        /// <value>
        /// The available connections.
        /// </value>
        IEnumerable<WebConnectionBase> AvailableConnections { get; }

        /// <summary>
        /// Gets the connection with specified name.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        /// <returns>Returns the connection with specified name; <c>null</c> if no connection with specified name is found.</returns>
        WebConnectionBase GetByName(string connectionName);
    }
}