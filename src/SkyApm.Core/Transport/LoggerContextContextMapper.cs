using SkyApm.Tracing.Segments;

namespace SkyApm.Transport
{
    public class LoggerContextContextMapper : ILoggerContextContextMapper
    {
        private readonly ISegmentContextMapper _segmentContextMapper;

        public LoggerContextContextMapper(ISegmentContextMapper segmentContextMapper)
        {
            _segmentContextMapper = segmentContextMapper;
        }

        public LoggerRequest Map(LoggerContext loggerContext)
        {
            var segmentRequest = _segmentContextMapper.Map(loggerContext.SegmentContext);
            return new LoggerRequest
            {
                Logs = loggerContext.Logs,
                SegmentRequest = segmentRequest,
            };
        }
    }
}
