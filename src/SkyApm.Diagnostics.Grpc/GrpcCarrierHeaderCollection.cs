using Grpc.Core;
using SkyApm.Tracing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyApm.Diagnostics.Grpc
{
    public class GrpcCarrierHeaderCollection : ICarrierHeaderCollection
    {
        private readonly Metadata _metadata;

        public GrpcCarrierHeaderCollection(Metadata metadata)
        {
            _metadata = metadata ?? new Metadata();
        }

        public void Add(string key, string value)
        {
            _metadata.Add(new Metadata.Entry(key, value));
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _metadata.Select(m => new KeyValuePair<string, string>(m.Key, m.Value)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _metadata.GetEnumerator();
        }
    }
}
