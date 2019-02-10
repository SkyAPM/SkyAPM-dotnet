using SkyWalking.Tracing.Segments;

namespace SkyWalking.Tracing
{
    public interface ICarrierParser
    {
        bool TryParse(string key, string content, out ICarrier carrier);

        bool TryParse(string key, SegmentContext segmentContext, out ICarrier carrier);
    }
}