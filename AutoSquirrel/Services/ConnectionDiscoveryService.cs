using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AutoSquirrel
{
    /// <summary>
    /// The connection discovery service.
    /// </summary>
    public class ConnectionDiscoveryService : IConnectionDiscoveryService
    {
        private Dictionary<string, WebConnectionBase> availableConnections;

        /// <summary>
        /// Gets the available connections.
        /// </summary>
        /// <value>
        /// The available connections.
        /// </value>
        /// <remarks>
        /// This will cache all available connections on first request.
        /// </remarks>
        public IEnumerable<WebConnectionBase> AvailableConnections =>
            this.availableConnections?.Values ??
            (this.availableConnections =
                Assembly
                    .GetExecutingAssembly()
                    .GetExportedTypes()
                    .Where(type => type.IsClass && !type.IsAbstract && typeof(WebConnectionBase).IsAssignableFrom(type))
                    .Select(connType => (WebConnectionBase)Activator.CreateInstance(connType))
                    .ToDictionary(conn => conn.ConnectionName, conn => conn)).Values;

        /// <summary>
        /// Gets the connection with specified name.
        /// </summary>
        /// <param name="connectionName">Name of the connection.</param>
        /// <returns>Returns the connection with specified name; <c>null</c> if no connection with specified name is found.</returns>
        public WebConnectionBase GetByName(string connectionName)
        {
            if (connectionName == null ||
                !this.availableConnections.ContainsKey(connectionName))
                return null;

            return this.availableConnections[connectionName];
        }
    }
}