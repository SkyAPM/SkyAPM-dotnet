using SkyApm.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Diagnostics.AspNetCore.Config
{
    [Config("SkyWalking", "Component", "AspNetCore")]
    public class HostingDiagnosticConfig
    {
        /// <summary>
        /// Auto collect specific cookies as span tag.
        /// </summary>
        public List<string> CollectCookies { get; set; }

        /// <summary>
        /// Auto collect specific headers as span tag
        /// </summary>
        public List<string> CollectHeaders { get; set; }

        /// <summary>
        /// Auto collect request body when Content-Type meets
        /// </summary>
        public List<string> CollectBodyContentTypes { get; set; }

        /// <summary>
        /// Request body will skip collecting if the Content-Length is larger than this value. 
        /// </summary>
        public int CollectBodyLengthThreshold { get; set; } = 2048;
    }
}
