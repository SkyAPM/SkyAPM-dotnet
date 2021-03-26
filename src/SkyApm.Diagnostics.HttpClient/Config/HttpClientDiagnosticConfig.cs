using SkyApm.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Diagnostics.HttpClient.Config
{
    [Config("SkyWalking", "Component", "HttpClient")]
    public class HttpClientDiagnosticConfig
    {
        /// <summary>
        /// Stop header propagation on specific paths, path support wildchar match.
        /// Usage: a/b/c => a/b/c, a/* => a/b, a/** => a/b/c/d, a/?/c => a/b/c
        /// </summary>
        public List<string> StopHeaderPropagationPaths { get; set; }

        /// <summary>
        /// Collect specific request headers as span tag
        /// </summary>
        public List<string> CollectRequestHeaders { get; set; }

        /// <summary>
        /// Collect request body as span tag for specific Content-Type
        /// </summary>
        public List<string> CollectRequestBodyContentTypes { get; set; }

        /// <summary>
        /// Collect response body as span tag for specific Content-Type
        /// </summary>
        public List<string> CollectResponseBodyContentTypes { get; set; }

        /// <summary>
        /// Request/Response body will skip collecting if the content length is larger than this value.
        /// </summary>
        public int CollectBodyLengthThreshold { get; set; } = 2048;

    }
}
