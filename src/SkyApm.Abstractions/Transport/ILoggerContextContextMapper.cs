using SkyApm.Tracing.Segments;
namespace SkyApm.Transport
{
    public interface ILoggerContextContextMapper
    {
        LoggerRequest Map(LoggerContext loggerContext);
    }
}
