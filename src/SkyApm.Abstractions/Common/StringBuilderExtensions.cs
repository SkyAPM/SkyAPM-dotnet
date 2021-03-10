using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Common
{
    internal static class StringBuilderExtensions
    {
        public static StringBuilder AppendDemystified(this StringBuilder builder, Exception exception)
        {
            try
            {
                builder.Append(exception.GetType());
                if (!string.IsNullOrEmpty(exception.Message))
                {
                    builder.Append(": ").Append(exception.Message);
                }

                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    builder.AppendLine();
                    builder.Append(exception.StackTrace);
                }

                if (exception is AggregateException aggEx)
                {
                    foreach (var ex in aggEx.InnerExceptions)
                    {
                        builder.AppendInnerException(ex);
                    }
                }

                if (exception.InnerException != null)
                {
                    builder.AppendInnerException(exception.InnerException);
                }
            }
            catch
            {
                // Processing exceptions shouldn't throw exceptions
            }

            return builder;
        }

        private static void AppendInnerException(this StringBuilder builder, Exception exception)
            => builder.AppendLine()
                .Append(" ---> ")
                .AppendDemystified(exception)
                .AppendLine()
                .Append("   --- End of inner exception stack trace ---");
    }
}
