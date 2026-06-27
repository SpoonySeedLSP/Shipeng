using System.ComponentModel;
using Shipeng.Dependency;

namespace Shipeng.EntityFrameworkCore
{
    /// <summary>
    /// ÅÅÐò·½Ê½
    /// </summary>
    [SuppressSniffer]
    public enum OrderType
    {
        /// <summary>
        /// ÉýÐò
        /// </summary>
        [Description("Ascending")] Ascending = 0,

        /// <summary>
        /// ½µÐò
        /// </summary>
        [Description("Descending")] Descending = 1
    }
}
