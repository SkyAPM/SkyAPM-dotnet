namespace SkyApm.Tracing;

public interface ICarrierHeaderDictionary : ICarrierHeaderCollection
{
    string Get(string key);
}