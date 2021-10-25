using System;

namespace LinkWheel.CodeHosts
{
    public class HostPriorityAttribute : Attribute
    {
        /// <summary>
        /// The priority this <see cref="RemoteRepoHost"/> has over others. Higher is better.
        /// </summary>
        public int Priority { get; set; }
        public HostPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
