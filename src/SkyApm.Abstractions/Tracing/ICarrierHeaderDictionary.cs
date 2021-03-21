using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Tracing
{
    public interface ICarrierHeaderDictionary : ICarrierHeaderCollection
    {
        string Get(string key);
    }
}
