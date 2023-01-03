using MassTransit;
using SkyApm.Tracing;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SkyApm.Diagnostics.MassTransit
{
    public class MasstransitCarrierHeaderCollection : ICarrierHeaderDictionary
    {
        private readonly Headers _headers;

        public MasstransitCarrierHeaderCollection(Headers headers)
        {
            _headers = headers;
        }
        public void Add(string key, string value)
        {
            //deal with GRPC transport later
            if (_headers.GetType().GetInterfaces().Contains(typeof(SendHeaders)))
            {
                ((SendHeaders)_headers).Set(key, value);
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        public string Get(string key)
        {
            if (_headers.TryGetHeader(key, out var value))
                return value.ToString();
            return null;
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _headers.Select(o => new KeyValuePair<string, string>(o.Key, o.Value.ToString())).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
