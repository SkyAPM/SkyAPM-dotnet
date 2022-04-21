using SkyApm.Tracing.Segments;

namespace SkyApm.Tracing
{
    public class CrossThreadCarrier : SegmentReference, ICarrier
    {
        public bool HasValue => true;

        public bool? Sampled { get; set; }
    }
}
