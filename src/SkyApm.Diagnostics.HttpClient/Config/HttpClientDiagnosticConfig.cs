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
    }
}
