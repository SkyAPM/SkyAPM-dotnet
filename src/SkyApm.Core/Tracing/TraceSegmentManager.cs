using SkyApm.Common;
using SkyApm.Config;
using SkyApm.Tracing.Segments;
using System.Threading;

namespace SkyApm.Tracing
{
    public class TraceSegmentManager : ITraceSegmentManager
    {
        private readonly AsyncLocal<WideTraceSegment> _traceSegments = new AsyncLocal<WideTraceSegment>();

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

        public TraceSegment ActiveSegment => _traceSegments.Value;

        public SegmentSpan CreateEntrySpan(string operationName, ICarrier carrier, long startTimeMilliseconds = 0)
        {
            SegmentReference segmentReference = null;
            var traceSegment = _traceSegments.Value;
            if (traceSegment == null)
            {
                var traceId = GetTraceId(carrier);
                var segmentId = GetSegmentId();
                var sampled = GetSampled(carrier, operationName);
                traceSegment = new WideTraceSegment(_uniqueIdGenerator, traceId, segmentId, sampled,
                    _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                    _instrumentConfig.ServiceInstanceName);

                if (carrier.HasValue)
                {
                    segmentReference = new SegmentReference
                    {
                        Reference = Reference.CrossProcess,
                        EntryEndpoint = carrier.EntryEndpoint,
                        NetworkAddress = carrier.NetworkAddress,
                        ParentEndpoint = carrier.ParentEndpoint,
                        ParentSpanId = carrier.ParentSpanId,
                        ParentSegmentId = carrier.ParentSegmentId,
                        EntryServiceInstanceId = carrier.EntryServiceInstanceId,
                        ParentServiceInstanceId = carrier.ParentServiceInstanceId,
                        TraceId = carrier.TraceId,
                        ParentServiceId = carrier.ParentServiceId,
                    };
                }

                _traceSegments.Value = traceSegment;
            }

            var span = traceSegment.CreateEntrySpan(operationName, startTimeMilliseconds);
            if (segmentReference != null)
            {
                span.References.Add(segmentReference);
            }
            return span;
        }

        public SegmentSpan CreateLocalSpan(string operationName, long startTimeMilliseconds = 0)
        {
            var traceSegment = _traceSegments.Value;
            if (traceSegment == null)
            {
                var traceId = GetTraceId(NullableCarrier.Instance);
                var segmentId = GetSegmentId();
                var sampled = GetSampled(NullableCarrier.Instance, operationName);
                traceSegment = new WideTraceSegment(_uniqueIdGenerator, traceId, segmentId, sampled,
                    _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                    _instrumentConfig.ServiceInstanceName);

                _traceSegments.Value = traceSegment;
            }

            return traceSegment.CreateLocalSpan(operationName, startTimeMilliseconds);
        }

        public SegmentSpan CreateLocalSpan(string operationName, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            if (carrier == null) return CreateLocalSpan(operationName, startTimeMilliseconds);

            var traceId = GetTraceId(carrier);
            var segmentId = GetSegmentId();
            var sampled = GetSampled(carrier, operationName);
            var traceSegment = new WideTraceSegment(_uniqueIdGenerator, traceId, segmentId, sampled,
                _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                _instrumentConfig.ServiceInstanceName);

            _traceSegments.Value = traceSegment;

            var span = traceSegment.CreateLocalSpan(operationName, startTimeMilliseconds);
            span.References.Add(carrier);
            return span;
        }

        public SegmentSpan CreateExitSpan(string operationName, StringOrIntValue networkAddress, long startTimeMilliseconds = 0)
        {
            var traceSegment = _traceSegments.Value;
            if (traceSegment == null)
            {
                var traceId = GetTraceId(NullableCarrier.Instance);
                var segmentId = GetSegmentId();
                var sampled = GetSampled(NullableCarrier.Instance, operationName);
                traceSegment = new WideTraceSegment(_uniqueIdGenerator, traceId, segmentId, sampled,
                    _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                    _instrumentConfig.ServiceInstanceName);

                _traceSegments.Value = traceSegment;
            }

            var span = traceSegment.CreateExitSpan(operationName, startTimeMilliseconds);
            span.Peer = networkAddress;
            return span;
        }

        public SegmentSpan CreateExitSpan(string operationName, StringOrIntValue networkAddress, CrossThreadCarrier carrier, long startTimeMilliseconds = 0)
        {
            if (carrier == null) return CreateExitSpan(operationName, networkAddress, startTimeMilliseconds);

            var traceId = GetTraceId(carrier);
            var segmentId = GetSegmentId();
            var sampled = GetSampled(carrier, operationName);
            var traceSegment = new WideTraceSegment(_uniqueIdGenerator, traceId, segmentId, sampled,
                _instrumentConfig.ServiceName ?? _instrumentConfig.ApplicationCode,
                _instrumentConfig.ServiceInstanceName);

            _traceSegments.Value = traceSegment;

            var span = traceSegment.CreateExitSpan(operationName, startTimeMilliseconds);
            span.Peer = networkAddress;
            span.References.Add(carrier);
            return span;
        }

        public TraceSegment StopSpan(SegmentSpan span, long endTimeMilliseconds = default)
        {
            var traceSegment = _traceSegments.Value;
            if (traceSegment == null) return null;

            var segment = traceSegment.Finish(span, endTimeMilliseconds);
            if (segment != null) _traceSegments.Value = null;

            return segment;
        }

        public (TraceSegment, SegmentSpan) StopSpan(long endTimeMilliseconds = 0)
        {
            var traceSegment = _traceSegments.Value;
            if (traceSegment == null) return (null, null);
            var span = traceSegment.CurrentSpan;
            _traceSegments.Value = null;
            return span == null ? (null, null) : (traceSegment.Finish(span, endTimeMilliseconds), span);
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
