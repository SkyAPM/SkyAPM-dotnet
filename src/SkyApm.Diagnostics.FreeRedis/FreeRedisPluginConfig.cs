/*
 * Licensed to the SkyAPM under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The SkyAPM licenses this file to You under the Apache License, Version 2.0
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

using System;
using System.Collections.Generic;
using System.Text;

namespace SkyApm.Diagnostics.FreeRedis
{
    internal static class FreeRedisPluginConfig
    {
        internal static HashSet<String> OPERATION_MAPPING_WRITE = new HashSet<string>() {
                    "GETSET",
                    "SET",
                    "SETBIT",
                    "SETEX ",
                    "SETNX ",
                    "SETRANGE",
                    "STRLEN ",
                    "MSET",
                    "MSETNX ",
                    "PSETEX",
                    "INCR ",
                    "INCRBY ",
                    "INCRBYFLOAT",
                    "DECR ",
                    "DECRBY ",
                    "APPEND ",
                    "HMSET",
                    "HSET",
                    "HSETNX ",
                    "HINCRBY",
                    "HINCRBYFLOAT",
                    "HDEL",
                    "RPOPLPUSH",
                    "RPUSH",
                    "RPUSHX",
                    "LPUSH",
                    "LPUSHX",
                    "LREM",
                    "LTRIM",
                    "LSET",
                    "BRPOPLPUSH",
                    "LINSERT",
                    "SADD",
                    "SDIFF",
                    "SDIFFSTORE",
                    "SINTERSTORE",
                    "SISMEMBER",
                    "SREM",
                    "SUNION",
                    "SUNIONSTORE",
                    "SINTER",
                    "ZADD",
                    "ZINCRBY",
                    "ZINTERSTORE",
                    "ZRANGE",
                    "ZRANGEBYLEX",
                    "ZRANGEBYSCORE",
                    "ZRANK",
                    "ZREM",
                    "ZREMRANGEBYLEX",
                    "ZREMRANGEBYRANK",
                    "ZREMRANGEBYSCORE",
                    "ZREVRANGE",
                    "ZREVRANGEBYSCORE",
                    "ZREVRANK",
                    "ZUNIONSTORE",
                    "XADD",
                    "XDEL",
                    "DEL",
                    "XTRIM"
                };
        internal static HashSet<String> OPERATION_MAPPING_READ = new HashSet<string>(){
                    "GET",
                    "GETRANGE",
                    "GETBIT ",
                    "MGET",
                    "HVALS",
                    "HKEYS",
                    "HLEN",
                    "HEXISTS",
                    "HGET",
                    "HGETALL",
                    "HMGET",
                    "BLPOP",
                    "BRPOP",
                    "LINDEX",
                    "LLEN",
                    "LPOP",
                    "LRANGE",
                    "RPOP",
                    "SCARD",
                    "SRANDMEMBER",
                    "SPOP",
                    "SSCAN",
                    "SMOVE",
                    "ZLEXCOUNT",
                    "ZSCORE",
                    "ZSCAN",
                    "ZCARD",
                    "ZCOUNT",
                    "XGET",
                    "GET",
                    "XREAD",
                    "XLEN",
                    "XRANGE",
                    "XREVRANGE"
            };
    }
}
