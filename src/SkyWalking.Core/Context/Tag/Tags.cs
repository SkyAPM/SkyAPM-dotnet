/*
 * Licensed to the OpenSkywalking under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace SkyWalking.Context.Tag
{
    /// <summary>
    /// The span tags are supported by sky-walking engine.
    /// As default, all tags will be stored, but these ones have particular meanings.
    /// </summary>
    public static class Tags
    {
        public static readonly StringTag Url = new StringTag("url");

        /// <summary>
        /// STATUS_CODE records the http status code of the response.
        /// </summary>
        public static readonly StringTag StatusCode = new StringTag("status_code");

        /// <summary>
        /// DB_TYPE records database type, such as sql, redis, cassandra and so on.
        /// </summary>
        public static readonly StringTag DbType = new StringTag("db.type");

        /// <summary>
        /// DB_INSTANCE records database instance name.
        /// </summary>
        public static readonly StringTag DbInstance = new StringTag("db.instance");

        /// <summary>
        /// DB_STATEMENT records the sql statement of the database access.
        /// </summary>
        public static readonly StringTag DbStatement = new StringTag("db.statement");

        /// <summary>
        /// DB_BIND_VARIABLES records the bind variables of sql statement.
        /// </summary>
        public static readonly StringTag DbBindVariables = new StringTag("db.bind_vars");

        /// <summary>
        /// MQ_BROKER records the broker address of message-middleware
        /// </summary>
        public static readonly StringTag MqBorker = new StringTag("mq.broker");

        /// <summary>
        /// MQ_TOPIC records the topic name of message-middleware
        /// </summary>
        public static readonly StringTag MqTopic = new StringTag("mq.topic");

        /// <summary>
        /// 
        /// </summary>
        public static readonly StringTag ErrorMessage = new StringTag("error.message");

        /// <summary>
        /// SpanKind hints at the relationship between spans, e.g. client/server.
        /// </summary>
        public static readonly StringTag SpanKind = new StringTag("span.kind");

        /// <summary>
        /// A static readonlyant for setting the span to indicate that it represents a producer span, in a messaging scenario.
        /// </summary>
        public static readonly StringTag MqProducer = new StringTag("producer");

        /// <summary>
        /// A static readonlyant for setting the span to indicate that it represents a consumer span, in a messaging scenario.
        /// </summary>
        public static readonly StringTag MqConsumer = new StringTag("consumer");

        ///// <summary>
        ///// MessageBusDestination records an address at which messages can be exchanged. E.g. A Kafka record has an
        ///// associated "topic name" that can be extracted by the instrumented producer or consumer and stored using this tag.
        ///// </summary>
        //public static readonly StringTag MessageBusDestination = new StringTag("message_bus.destination");

        public static class HTTP
        {
            public static readonly StringTag Method = new StringTag("http.method");
        }
    }
}