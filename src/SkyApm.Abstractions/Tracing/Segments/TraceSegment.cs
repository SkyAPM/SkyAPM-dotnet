using System.Collections.Generic;

namespace SkyApm.Tracing.Segments
{
    public class TraceSegment
    {
        private SegmentSpan _firstSpan;

        public string SegmentId { get; set; }

        public string TraceId { get; set; }

        public List<SegmentSpan> Spans { get; } = new List<SegmentSpan>();

        public string ServiceId { get; set; }

        public string ServiceInstanceId { get; set; }

        public bool Sampled { get; set; }

        public bool IsSizeLimited { get; set; } = false;

        public SegmentSpan FirstSpan
        {
            get => _firstSpan;
            set
            {
                _firstSpan = value;
                _firstSpan.Segment = this;
            }
        }

        public TraceSegment(string traceId, string segmentId, bool sampled, string serviceId, string serviceInstanceId)
        {
            TraceId = traceId;
            Sampled = sampled;
            SegmentId = segmentId;
            ServiceId = serviceId;
            ServiceInstanceId = serviceInstanceId;
        }
    }
}
