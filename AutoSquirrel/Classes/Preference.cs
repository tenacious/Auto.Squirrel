using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AutoSquirrel
{
    /// <summary>
    /// Preference
    /// </summary>
    [DataContract]
    public class Preference
    {
        /// <summary>
        /// The last opened project
        /// </summary>
        [DataMember]
        public List<string> LastOpenedProject = new List<string>();
    }
}
