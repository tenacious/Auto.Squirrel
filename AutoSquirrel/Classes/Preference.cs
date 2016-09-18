namespace AutoSquirrel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract]
    public class Preference
    {
        [DataMember]
        public List<string> LastOpenedProject = new List<string>();
    }
}