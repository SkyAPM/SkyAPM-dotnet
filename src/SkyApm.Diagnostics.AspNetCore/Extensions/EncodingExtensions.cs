using System.Text;

namespace SkyApm.Diagnostics.AspNetCore.Extensions;

internal static class EncodingExtensions
{
    public static Encoding ToEncoding(this string charset, Encoding fallbackDefault)
    {
        try {
            return string.IsNullOrEmpty(charset) ? fallbackDefault : Encoding.GetEncoding(charset);
        }
        catch (Exception)
        {
            return fallbackDefault;
        }
    }
}