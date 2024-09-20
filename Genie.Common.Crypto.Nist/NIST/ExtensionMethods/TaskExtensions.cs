using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace NIST.CVP.ACVTS.Libraries.Common.ExtensionMethods
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Useful for firing off tasks and not awaiting the result
        /// </summary>
        /// <param name="task"></param>
        public static void FireAndForget(this Task task) { }
    }
}
