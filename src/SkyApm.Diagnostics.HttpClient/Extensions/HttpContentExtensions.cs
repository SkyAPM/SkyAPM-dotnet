using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace SkyApm.Diagnostics.HttpClient.Extensions
{
    internal static class HttpContentExtensions
    {
        public static string TryCollectAsString(this HttpContent httpContent, IEnumerable<string> contentTypeFilter, int lengthThreshold)
        {
            if (httpContent == null || httpContent.Headers.ContentLength > lengthThreshold)
                return null;

            var mediaHeader = httpContent.Headers.ContentType;
            if (mediaHeader == null || !contentTypeFilter.Any(supportedType => mediaHeader.MediaType == supportedType))
            {
                return null;
            }

            try
            {
                var responseBody = httpContent.ReadAsStringAsync().Result;
                // after ReadAsString(), the content length will be filled, in case of the Content-Length did not present in http header,
                // so we recheck the skip threhold
                if (httpContent.Headers.ContentLength > lengthThreshold)
                    return null;
                return responseBody;
            }
            catch
            {
                return null;
            }
        }
    }
}
