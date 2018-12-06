
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
namespace SkyWalking.Diagnostics.CSRedis
{
    /// <summary>

    /// </summary>
    public static class CSRedisDiagnosticListenerExtensions
    {
        public const string DiagnosticListenerName = "CSRedisDiagnosticListener";

        private const string CSRedisPrefix = "DotNetCore.CSRedis.";

        public const string CSRedisBeforePublishMessageStore = CSRedisPrefix + nameof(WritePublishMessageStoreBefore);
        public const string CSRedisAfterPublishMessageStore = CSRedisPrefix + nameof(WritePublishMessageStoreAfter);
        public const string CSRedisErrorPublishMessageStore = CSRedisPrefix + nameof(WritePublishMessageStoreError);


        public static Guid WritePublishMessageStoreBefore(this DiagnosticListener @this,
            CSRedisPublishedMessage message,
            [CallerMemberName] string operation = "")
        {
            if (@this.IsEnabled(CSRedisBeforePublishMessageStore))
            {
                var operationId = Guid.NewGuid();
                BrokerPublishEventData eventData = new BrokerPublishEventData(new Guid(),"","","","",DateTime.Now);
                eventData.Headers = new TracingHeaders();
                @this.Write(CSRedisBeforePublishMessageStore, eventData);
                //@this.Write(CSRedisBeforePublishMessageStore, new
                //{
                //    OperationId = operationId,
                //    Operation = operation,
                //    MessageName = message.Name,
                //    MessageContent = message.Content
                //});

                return operationId;
            }

            return Guid.Empty;
        }

        public static void WritePublishMessageStoreAfter(this DiagnosticListener @this,
            Guid operationId,
            CSRedisPublishedMessage message,
            [CallerMemberName] string operation = "")
        {
            if (@this.IsEnabled(CSRedisAfterPublishMessageStore))
            {
                @this.Write(CSRedisAfterPublishMessageStore, new
                {
                    OperationId = operationId,
                    Operation = operation,
                    MessageId = message.Id,
                    MessageName = message.Name,
                    MessageContent = message.Content,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }

        public static void WritePublishMessageStoreError(this DiagnosticListener @this,
            Guid operationId,
            CSRedisPublishedMessage message,
            Exception ex,
            [CallerMemberName] string operation = "")
        {
            if (@this.IsEnabled(CSRedisErrorPublishMessageStore))
            {
                @this.Write(CSRedisErrorPublishMessageStore, new
                {
                    OperationId = operationId,
                    Operation = operation,
                    MessageName = message.Name,
                    MessageContent = message.Content,
                    Exception = ex,
                    Timestamp = Stopwatch.GetTimestamp()
                });
            }
        }



    }
}