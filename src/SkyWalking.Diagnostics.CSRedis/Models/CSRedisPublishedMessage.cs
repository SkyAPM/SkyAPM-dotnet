using System;
using System.Collections.Generic;
using System.Text;

namespace SkyWalking.Diagnostics.CSRedis
{
  
    public class CSRedisPublishedMessage
    {
        /// <summary>
        /// Initializes a new instance of <see cref="CapPublishedMessage" />.
        /// </summary>
        public CSRedisPublishedMessage()
        {
            Added = DateTime.Now;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Content { get; set; }

        public DateTime Added { get; set; }

        public DateTime? ExpiresAt { get; set; }

        public int Retries { get; set; }

        public string StatusName { get; set; }

        public override string ToString()
        {
            return "name:" + Name + ", content:" + Content;
        }
    }
}
