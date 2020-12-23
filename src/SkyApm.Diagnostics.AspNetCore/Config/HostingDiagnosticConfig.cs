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
        /// Auto collect specific cookies as span tags.
        /// </summary>
        public List<string> AutoTagCookies { get; set; }
    }
}
