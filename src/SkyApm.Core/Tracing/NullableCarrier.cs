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

using SkyApm.Common;

namespace SkyApm.Tracing;

public class NullableCarrier : ICarrier
{
    public static NullableCarrier Instance { get; } = new();
        
    public bool HasValue { get; } = false;
        
    public bool? Sampled { get; }
        
    public string TraceId { get; }
        
    public string ParentSegmentId { get; }
        
    public int ParentSpanId { get; }
        
    public string ParentServiceInstanceId { get; }
        
    public string EntryServiceInstanceId { get; }
        
    public StringOrIntValue NetworkAddress { get; }
        
    public StringOrIntValue EntryEndpoint { get; }
        
    public StringOrIntValue ParentEndpoint { get; }

    public string ParentServiceId { get; }
}