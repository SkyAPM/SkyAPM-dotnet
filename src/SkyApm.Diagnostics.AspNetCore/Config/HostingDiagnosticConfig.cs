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
    }
}
