using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing.Segments;
using System.Threading;

namespace SkyApm.Tracing
{
    public class TraceSegmentManager : ITraceSegmentManager
    {
        private readonly AsyncLocal<WideTraceSegment> _traceSegments = new AsyncLocal<WideTraceSegment>();
        private readonly AsyncLocal<SegmentSpan> _currentSpanRecord = new AsyncLocal<SegmentSpan>();

        private readonly ISamplerChainBuilder _samplerChainBuilder;
        private readonly IUniqueIdGenerator _uniqueIdGenerator;
        private readonly InstrumentConfig _instrumentConfig;

        public TraceSegmentManager(ISamplerChainBuilder samplerChainBuilder, IUniqueIdGenerator uniqueIdGenerator, IConfigAccessor configAccessor)
        {
            _samplerChainBuilder = samplerChainBuilder;
            _uniqueIdGenerator = uniqueIdGenerator;
            _instrumentConfig = configAccessor.Get<InstrumentConfig>();
        }

        public SegmentSpan ActiveSpan => _traceSegments.Value?.CurrentSpan;

        public SegmentSpan CreateEntrySpan(string operationName, ICarrier carrier, long startTimeMilliseconds = 0)
        {
            SegmentReference segmentReference = null;
            var traceSegment = _traceSegments.Value;
            // Ignore carrier when TraceSegment already exists.
            if (traceSegment == null)
            {
                traceSegment = CreateSegment(operationName, carrier);

                segmentReference = carrier.ToReference();

                _traceSegments.Value = traceSegment;
            }

            var span = traceSegment.CreateEntrySpan(operationName, startTimeMilliseconds);
            if (span == null)
            {
                // The parent span is complete
                if(_currentSpanRecord.Value == null)
                {
                    // Create a new segment to associate with carrier.
                    segmentReference = carrier.ToReference();
                }
                else
                {
                    // Create a new segment to associate with parent.
                    var crossThreadCarrier = _currentSpanRecord.Value.GetCrossThreadCarrier();
                    carrier = crossThreadCarrier;
                    segmentReference = crossThreadCarrier;
                }
                traceSegment = CreateSegment(operationName, carrier);
                span = traceSegment.CreateEntrySpan(operationName, startTimeMilliseconds);
                _traceSegments.Value = traceSegment;
            }
            if (segmentReference != null)
            {
                span.References.Add(segmentReference);
            }
            _currentSpanRecord.Value = span;
            return span;
        }

        public SegmentSpan CreateLocalSpan(string operationName, long startTimeMilliseconds = 0)
        {
            var traceSegment = _traceSegments.Value;
            if (traceSegment == null)
            {
                traceSegment = CreateSegment(operationName, NullableCarrier.Instance);

                _traceSegments.Value = traceSegment;
            }

            var span = traceSegment.CreateLocalSpan(operationName, startTimeMilliseconds);
            if (span == null)
            {
                // The parent span is complete, try create a new segment to associate with parent.
                var carrier = _currentSpanRecord.Value.GetCrossThreadCarrier();
                traceSegment = CreateSegment(operationName, carrier);
                span = traceSegment.CreateEntrySpan(operationName, startTimeMilliseconds);
                if(carrier != null)
                {
                    span.References.Add(carrier);
                }
                _traceSegments.Value = traceSegment;
            }
            _currentSpanRecord.Value = span;
            return span;
        }

        public SegmentSpan CreateLocalSpan(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            if (carrier == null) return CreateLocalSpan(operationName, startTimeMilliseconds);

            if (!carrier.NetworkAddress.HasValue) carrier.NetworkAddress = "UNKNOW";
            var traceSegment = CreateSegment(operationName, carrier);

            _traceSegments.Value = traceSegment;

            var span = traceSegment.CreateLocalSpan(operationName, startTimeMilliseconds);
            span.References.Add(carrier);
            _currentSpanRecord.Value = span;
            return span;
        }

        public SegmentSpan CreateExitSpan(string operationName, StringOrIntValue networkAddress, long startTimeMilliseconds = 0)
        {
            var traceSegment = _traceSegments.Value;
            if (traceSegment == null)
            {
                traceSegment = CreateSegment(operationName, NullableCarrier.Instance);

                _traceSegments.Value = traceSegment;
            }

            var span = traceSegment.CreateExitSpan(operationName, startTimeMilliseconds);
            if (span == null)
            {
                // The parent span is complete, try create a new segment to associate with parent.
                var carrier = _currentSpanRecord.Value.GetCrossThreadCarrier();
                traceSegment = CreateSegment(operationName, carrier);
                span = traceSegment.CreateEntrySpan(operationName, startTimeMilliseconds);
                if (carrier != null)
                {
                    span.References.Add(carrier);
                }
                _traceSegments.Value = traceSegment;
            }
            span.Peer = networkAddress;
            _currentSpanRecord.Value = span;
            return span;
        }

        public SegmentSpan CreateExitSpan(string operationName, StringOrIntValue networkAddress, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            if (carrier == null) return CreateExitSpan(operationName, networkAddress, startTimeMilliseconds);

            if (!carrier.NetworkAddress.HasValue) carrier.NetworkAddress = "UNKNOW";
            var traceSegment = CreateSegment(operationName, carrier);

            _traceSegments.Value = traceSegment;

            var span = traceSegment.CreateExitSpan(operationName, startTimeMilliseconds);
            span.Peer = networkAddress;
            span.References.Add(carrier);
            _currentSpanRecord.Value = span;
            return span;
        }

        public TraceSegment StopSpan(SegmentSpan span, long endTimeMilliseconds = default)
        {
            TraceSegment segment = null;
            var traceSegment = _traceSegments.Value;
            if (traceSegment != null)
            {
                segment = traceSegment.Finish(span, endTimeMilliseconds);
                if (segment != null) _traceSegments.Value = null;
            }

            _currentSpanRecord.Value = span.Parent;
            return segment;
        }

        public (TraceSegment, SegmentSpan) StopSpan(long endTimeMilliseconds = 0)
        {
            TraceSegment segment = null;
            SegmentSpan span = null;
            var traceSegment = _traceSegments.Value;
            if (traceSegment != null)
            {
                span = traceSegment.CurrentSpan;
                _traceSegments.Value = null;
                if(span != null)
                {
                    segment = traceSegment.Finish(span, endTimeMilliseconds);
                }
            }

            _currentSpanRecord.Value = span?.Parent;
            return (segment, span);
        }

        private WideTraceSegment CreateSegment(string operationName, ICarrier carrier)
        {
            if (carrier == null) carrier = NullableCarrier.Instance;

            var traceId = GetTraceId(carrier);
            var segmentId = GetSegmentId();
            var sampled = GetSampled(carrier, operationName);
            var traceSegment = new WideTraceSegment(_uniqueIdGenerator, traceId, segmentId, sampled,
                _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                _instrumentConfig.ServiceInstanceName);

            return traceSegment;
        }

        private string GetTraceId(ICarrier carrier)
        {
            return carrier.HasValue ? carrier.TraceId : _uniqueIdGenerator.Generate();
        }

        private string GetSegmentId()
        {
            return _uniqueIdGenerator.Generate();
        }

        private bool GetSampled(ICarrier carrier, string operationName)
        {
            if (carrier.HasValue && carrier.Sampled.HasValue)
            {
                return carrier.Sampled.Value;
            }

            SamplingContext samplingContext;
            if (carrier.HasValue)
            {
                samplingContext = new SamplingContext(operationName, carrier.NetworkAddress, carrier.EntryEndpoint,
                    carrier.ParentEndpoint);
            }
            else
            {
                samplingContext = new SamplingContext(operationName, default(StringOrIntValue), default(StringOrIntValue),
                    default(StringOrIntValue));
            }

            var sampler = _samplerChainBuilder.Build();
            return sampler(samplingContext);
        }
    }
}
