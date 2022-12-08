namespace SkyApm.Diagnostics.HttpClient.Extensions;

internal static class HttpContentExtensions
{
    public static string TryCollectAsString(this HttpContent httpContent, IEnumerable<string> contentTypeFilter, int lengthThreshold)
    {
        if (httpContent is null || httpContent.Headers.ContentLength > lengthThreshold)
            return null;

        var mediaHeader = httpContent.Headers.ContentType;
        if (mediaHeader is null || !contentTypeFilter.Any(supportedType => mediaHeader.MediaType == supportedType))
        {
            return null;
        }

        try
        {
            var responseBody = httpContent.ReadAsStringAsync().Result;
            // after ReadAsString(), the content length will be filled, in case of the Content-Length did not present in http header,
            // so we recheck the skip threhold
            return httpContent.Headers.ContentLength > lengthThreshold ? null : responseBody;
        }
        catch
        {
            return null;
        }
    }
}