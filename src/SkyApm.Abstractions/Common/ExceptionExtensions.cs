using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Common
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// Gets demystified string representation of the <paramref name="exception"/>.
        /// </summary>
        /// <param name="exception"></param>
        /// <param name="maxDepth"></param>
        /// <returns></returns>
        public static string ToDemystifiedString(this Exception exception, int maxDepth = 3)
        {
            var builder = new StringBuilder();
            builder.Append(exception, maxDepth);
            return builder.ToString();
        }

        /// <summary>
        /// Check if the exception has inner exceptions
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static bool HasInnerExceptions(this Exception exception)
        {
            return exception.InnerException != null || exception is AggregateException;
        }
    }
}
